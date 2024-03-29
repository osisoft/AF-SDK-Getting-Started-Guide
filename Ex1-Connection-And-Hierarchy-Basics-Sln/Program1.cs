﻿using System;
using System.IO;
using System.Text.Json;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.UnitsOfMeasure;

namespace Ex1ConnectionAndHierarchyBasicsSln
{
    public static class Program1
    {
        public static string AFServer { get; set; }
        public static string DatabaseString { get; set; }

        public static void Main()
        {
            Setup();
            AFDatabase database = GetDatabase(AFServer, DatabaseString);

            if (database == null) throw new NullReferenceException("Database is null");

            PrintRootElements(database);
            PrintElementTemplates(database);
            PrintAttributeTemplates(database, "MeterAdvanced");
            PrintEnergyUOMs(database.PISystem);
            PrintEnumerationSets(database);
            PrintCategories(database);

            Console.WriteLine("Press ENTER key to close");
            Console.ReadLine();
        }

        public static AFDatabase GetDatabase(string server, string database)
        {
            PISystems piSystems = new PISystems();
            PISystem assetServer = piSystems[server];
            AFDatabase afDatabase = assetServer.Databases[database];
            return afDatabase;
        }

        public static void PrintRootElements(AFDatabase database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            Console.WriteLine("Print Root Elements: {0}", database.Elements.Count);
            foreach (AFElement element in database.Elements)
            {
                Console.WriteLine("  {0}", element.Name);
            }

            Console.WriteLine();
        }

        public static void PrintElementTemplates(AFDatabase database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            Console.WriteLine("Print Element Templates");
            AFNamedCollectionList<AFElementTemplate> elemTemplates = database.ElementTemplates.FilterBy(typeof(AFElement));
            foreach (AFElementTemplate elemTemp in elemTemplates)
            {
                Console.WriteLine("Name: {0}; Categories: {1}", elemTemp.Name, elemTemp.CategoriesString);
            }

            Console.WriteLine();
        }

        public static void PrintAttributeTemplates(AFDatabase database, string elemTempName)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            Console.WriteLine("Print Attribute Templates for Element Template: {0}", elemTempName);
            AFElementTemplate elemTemp = database.ElementTemplates[elemTempName];
            foreach (AFAttributeTemplate attrTemp in elemTemp.AttributeTemplates)
            {
                string drName = attrTemp.DataReferencePlugIn == null ? "None" : attrTemp.DataReferencePlugIn.Name;
                Console.WriteLine("Name: {0}, DRPlugin: {1}", attrTemp.Name, drName);
            }

            Console.WriteLine();
        }

        public static void PrintEnergyUOMs(PISystem system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            Console.WriteLine("Print Energy UOMs");
            UOMClass uomClass = system.UOMDatabase.UOMClasses["Energy"];
            foreach (UOM uom in uomClass.UOMs)
            {
                Console.WriteLine("UOM: {0}, Abbreviation: {1}", uom.Name, uom.Abbreviation);
            }

            Console.WriteLine();
        }

        public static void PrintEnumerationSets(AFDatabase database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            Console.WriteLine("Print Enumeration Sets");
            AFEnumerationSets enumSets = database.EnumerationSets;
            foreach (AFEnumerationSet enumSet in enumSets)
            {
                Console.WriteLine(enumSet.Name);
                foreach (AFEnumerationValue state in enumSet)
                {
                    Console.WriteLine("{0} - {1}", state.Value, state.Name);
                }

                Console.WriteLine();
            }
        }

        public static void PrintCategories(AFDatabase database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            Console.WriteLine("Print Categories");
            Console.WriteLine("Element Categories");
            foreach (AFCategory category in database.ElementCategories)
            {
                Console.WriteLine(category.Name);
            }

            Console.WriteLine();
            Console.WriteLine("Attribute Categories");
            foreach (AFCategory category in database.AttributeCategories)
            {
                Console.WriteLine(category.Name);
            }

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
