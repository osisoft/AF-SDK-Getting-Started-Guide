﻿using System;
using System.IO;
using System.Text.Json;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Search;

namespace Ex2SearchingForAssetsSln
{
    public static class Program2
    {
        public static string AFServer { get; set; }
        public static string DatabaseString { get; set; }

        public static void Main()
        {
            Setup();
            AFDatabase database = GetDatabase(AFServer, DatabaseString);

            if (database == null) throw new NullReferenceException("Database is null");

            FindMetersByName(database, "Meter00*");
            FindMetersByTemplate(database, "MeterBasic");
            FindMetersBySubstation(database, "SSA*");
            FindMetersAboveUsage(database, 300);
            FindBuildingInfo(database, "MeterAdvanced");

            Console.WriteLine("Press ENTER key to close");
            Console.ReadLine();
        }

        public static AFDatabase GetDatabase(string serverName, string databaseName)
        {
            PISystems systems = new PISystems();
            PISystem assetServer;

            if (!string.IsNullOrEmpty(serverName))
                assetServer = systems[serverName];
            else
                assetServer = systems.DefaultPISystem;

            if (!string.IsNullOrEmpty(databaseName))
                return assetServer.Databases[databaseName];
            else
                return assetServer.Databases.DefaultDatabase;
        }

        public static void FindMetersByName(AFDatabase database, string elementNameFilter)
        {
            Console.WriteLine("Find Meters by Name: {0}", elementNameFilter);

            // Default search is as an element name string mask.
            var queryString = $"\"{elementNameFilter}\"";
            using (AFElementSearch elementQuery = new AFElementSearch(database, "ElementSearch", queryString))
            {
                elementQuery.CacheTimeout = TimeSpan.FromMinutes(5);
                foreach (AFElement element in elementQuery.FindObjects())
                {
                    Console.WriteLine("Element: {0}, Template: {1}, Categories: {2}",
                        element.Name,
                        element.Template.Name,
                        element.CategoriesString);
                }
            }

            Console.WriteLine();
        }

        public static void FindMetersByTemplate(AFDatabase database, string templateName)
        {
            Console.WriteLine("Find Meters by Template: {0}", templateName);

            using (AFElementSearch elementQuery = new AFElementSearch(database, "TemplateSearch", string.Format("template:\"{0}\"", templateName)))
            {
                elementQuery.CacheTimeout = TimeSpan.FromMinutes(5);
                int countDerived = 0;
                foreach (AFElement element in elementQuery.FindObjects())
                {
                    Console.WriteLine("Element: {0}, Template: {1}", element.Name, element.Template.Name);
                    if (element.Template.Name != templateName)
                        countDerived++;
                }

                Console.WriteLine("   Found {0} derived templates", countDerived);
                Console.WriteLine();
            }
        }

        public static void FindMetersBySubstation(AFDatabase database, string substationLocation)
        {
            Console.WriteLine("Find Meters by Substation: {0}", substationLocation);

            string templateName = "MeterBasic";
            string attributeName = "Substation";
            using (AFElementSearch elementQuery = new AFElementSearch(database, "AttributeValueEQSearch",
                string.Format("template:\"{0}\" \"|{1}\":\"{2}\"", templateName, attributeName, substationLocation)))
            {
                elementQuery.CacheTimeout = TimeSpan.FromMinutes(5);
                int countNames = 0;
                foreach (AFElement element in elementQuery.FindObjects())
                {
                    Console.Write("{0}{1}", countNames++ == 0 ? string.Empty : ", ", element.Name);
                }

                Console.WriteLine(String.Empty);
            }
        }

        public static void FindMetersAboveUsage(AFDatabase database, double val)
        {
            Console.WriteLine("Find Meters above Usage: {0}", val);

            string templateName = "MeterBasic";
            string attributeName = "Energy Usage";
            using (AFElementSearch elementQuery = new AFElementSearch(database, "AttributeValueGTSearch",
                string.Format("template:\"{0}\" \"|{1}\":>{2}", templateName, attributeName, val)))
            {
                elementQuery.CacheTimeout = TimeSpan.FromMinutes(5);
                int countNames = 0;
                foreach (AFElement element in elementQuery.FindObjects())
                {
                    Console.Write("{0}{1}", countNames++ == 0 ? string.Empty : ", ", element.Name);
                }

                Console.WriteLine(String.Empty);
            }
        }

        public static void FindBuildingInfo(AFDatabase database, string templateName)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            Console.WriteLine("Find Building Info: {0}", templateName);

            AFCategory buildingInfoCat = database.AttributeCategories["Building Info"];
            AFNamedCollectionList<AFAttribute> foundAttributes = new AFNamedCollectionList<AFAttribute>();

            using (AFElementSearch elementQuery = new AFElementSearch(database, "AttributeCattegorySearch", string.Format("template:\"{0}\"", templateName)))
            {
                elementQuery.CacheTimeout = TimeSpan.FromMinutes(5);
                foreach (AFElement element in elementQuery.FindObjects())
                {
                    foreach (AFAttribute attr in element.Attributes)
                    {
                        if (attr.Categories.Contains(buildingInfoCat))
                        {
                            foundAttributes.Add(attr);
                        }
                    }
                }
            }

            Console.WriteLine("Found {0} attributes.", foundAttributes.Count);
            Console.WriteLine();
        }

        public static void Setup()
        {
            AppSettings settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(Directory.GetCurrentDirectory() + "/appsettings.json"));

            // ==== Client constants ====
            AFServer = settings.AFServerName;
            DatabaseString = settings.AFDatabaseName;
        }
    }
}
