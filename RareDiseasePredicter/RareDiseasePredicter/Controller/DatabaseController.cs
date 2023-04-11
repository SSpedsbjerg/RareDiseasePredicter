using Microsoft.Data.Sqlite;
using RareDiseasePredicter.Enums;
using RareDiseasePredicter.Implementations;
using RareDiseasePredicter.Interfaces;
using System.Data;

namespace RareDiseasePredicter.Controller {
    static class DatabaseController {

        private static SqliteConnection Connection;

        private static bool CreateConnection() {
            if (Connection == null) {
                try {
                    Connection = new SqliteConnection("Data Source = Database.sqlite;Version=3;");
                    Connection.Open();
                    return true;
                }
                catch {
                    return false;
                }
            }
            else {
                return true;
            }
        }

        private static bool CreateTables() {
            try {
                string createQuery;
                createQuery = "CREATE TABLE DiseaseSymptomsReference (ID int identity(1,1) not null primary key," +
                    "DiseaseID int not null," +
                    "SymptomID int not null);";
                var command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();

                createQuery = "CREATE TABLE Disease" +
                    " (ID int identity(1,1) not null primary key," +
                    " Description VARCHAR(MAX)," +
                    " Href VARCHAR(MAX)," +
                    " Name VARCHAR(512)" +
                    " );";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();

                createQuery = "CREATE TABLE RegionSymptoms (" +
                    "ID int identity(1,1) not null primary key," +
                    "SymptomRef int not null," +
                    "RegionRef int not null);";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();

                createQuery = "CREATE TABLE Regions (" +
                    " ID int identity(1,1) not null primary key," +
                    " Name VARCHAR(512)," +
                    " SympRef int not null);";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();

                //TODO: Update Region to reference RegionSymptoms
                createQuery = "CREATE TABLE Symptoms (" +
                    " ID int identity(1,1) not null primary key," +
                    " Region int FOREIGN KEY REFERENCES Regions(ID)," +
                    " Name VARCHAR(MAX)," +
                    " Description VARCHAR(MAX)," +
                    " );";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();
                return true;
            }
            catch {
                return false;
            }

        }

        //TODO: This is just for dummy data and should be connected to a database
        //Deprecated, use GetSymptomsAsync
        public static async Task<ICollection<ISymptom>> getSymptoms() {
            List<ISymptom> symptoms = new List<ISymptom>();
            symptoms.Add(new Symptom("Hovdepine", new List<Region> { Region.Head }, 0, "Smerte i hjernen"));
            symptoms.Add(new Symptom("Smerte", new List<Region> { Region.Head, Region.Face, Region.Lips, Region.Mouth, Region.Ear, Region.Neck, Region.Shoulders }, 1, "Smerte"));
            return symptoms;
            }

        public static async Task<ICollection<ISymptom>> GetSymptomsAsync() {
            if(CreateConnection()) {

            }
            else {
                return null;
            }
            List<ISymptom> symptoms = new List<ISymptom>();

            return null;
        }

        public static async Task<bool> AddDiseaseAsync(IDisease disease) {
            string query;
            var command = Connection.CreateCommand();
            if(CreateConnection()) {
                foreach(ISymptom symptom in disease.GetSymptoms()) {
                    query = $"SELECT * FROM Symptoms WHERE ID = {symptom.ID};";
                    command.CommandText = query;
                    using (var reader = command.ExecuteReader()) {
                        bool found = false;
                        while(await reader.ReadAsync()) {
                            if (symptom.ID == reader.GetInt32(0)) {
                                found = true;
                            }
                        }
                        if(!found) {
                            throw new Exception($"{disease.Name} : {disease.ID} contains a Symptom ({symptom.Name} : {symptom.ID}) which does not have a maching ID with any other symptoms in the database");
                        }
                    }
                }
                foreach(ISymptom symptom in disease.GetSymptoms()) {
                    query = $"INSERT INTO DiseaseSymptomsReference ({disease.ID}, {symptom.ID});";
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();
                }
                query = $"INSERT INTO Disease ({disease.ID}, {disease.Description}, {disease.Href}, {disease.Name});";
                command.CommandText = query;
                await command.ExecuteNonQueryAsync();
            }
            else {
                return false;
            }
            return true;
        }

        public static async Task<bool> AddSymptomAsync(ISymptom symptom) {
            try {
                var command = Connection.CreateCommand();
                foreach(IRegion region in symptom.Regions) {
                    if(await AddRegionAsync(region)) { Console.WriteLine($"Added {region.Name} to the database"); }
                }
                List<IRegion> regions = (List<IRegion>)await GetRegionsAsync();
                command.CommandText = "SELECT SymptomRef FROM RegionSymptoms";
                int lastRefID = -1;
                using(var reader = command.ExecuteReader()) {
                    while(await reader.ReadAsync()) {
                        lastRefID = reader.GetInt32(0);
                    }
                }
                lastRefID++;
                command.CommandText = $"INSERT INTO Symptoms {lastRefID}, {symptom.Name}, {symptom.Description}";
                var query = command.ExecuteNonQueryAsync();
                foreach(IRegion region in regions) {
                    foreach(IRegion sympRegion in symptom.Regions) {
                        if(region.ID == sympRegion.ID) {
                            AddSympRegionReferenceAsync(sympRegion.ID, region.ID);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        //Checks if the reference already exist, if not add it
        private static async Task<bool> AddSympRegionReferenceAsync(int sympID, int regionID) {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT * FROM RegionSymptoms";
            int id0 = -1;
            int id1 = -1;
            using ( var reader = command.ExecuteReader()) {
                while (await reader.ReadAsync()) {
                    id0 = reader.GetInt32(0);
                    id1 = reader.GetInt32(1);
                    if(id0 == sympID && id1 == regionID) return false;
                }
            }
            if(id0 == -1 || id1 == -1) return false;
            command.CommandText = $"INSERT INTO RegionSymptoms {sympID}, {regionID}";
            command.ExecuteNonQuery();
            return true;
        }

        //Read all of the regions and if any matches, don't add it
        public static async Task<bool> AddRegionAsync(IRegion region) {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT Name FROM Regions";
            using (var reader = command.ExecuteReader()) {
                while(await reader.ReadAsync()) {
                    if (reader.GetString(0) == region.Name) {
                        return false;
                    }
                }

                command.CommandText = "SELECT SymptomRef FROM RegionSymptoms";
                int lastRefID = -1;
                using(var reader_ = command.ExecuteReader()) {
                    while(await reader_.ReadAsync()) {
                        lastRefID = reader_.GetInt32(1);
                    }
                }
                lastRefID++;

                command.CommandText = $"INSERT INTO Regions {region.Name}, {lastRefID};";
                await command.ExecuteNonQueryAsync();
                return true;
            }
        }

        public static async Task<ICollection<IRegion>> GetRegionsAsync() {
            List<IRegion> regions = new List<IRegion>();
            var command = Connection.CreateCommand();
            using (var reader = command.ExecuteReader()) {  
                while(await reader.ReadAsync()) {
                    regions.Add(new Implementations.Region(reader.GetString(1), reader.GetInt32(0)));
                }
            }
            return regions;
        }

        //TODO: This is just for dummy data and should be connected to a database
        public static async Task<ICollection<IDisease>> getDiseases(){
            List<IDisease> diseases = new List<IDisease>();
            diseases.Add(new Disease("Steven Johnson",
                new List<ISymptom> {
                    new Symptom("Smerte",
                    new List<Region> {
                        Region.Head,
                        Region.Face,
                        Region.Lips,
                        Region.Mouth,
                        Region.Ear,
                        Region.Neck },
                    1,
                    "Smerte er lav, men over det hele" )},
                0,
                "Kommer ofte fra medicin",
                "https://www.nhs.uk/conditions/stevens-johnson-syndrome/"
                ));

            diseases.Add(new Disease("Pompes",
                new List<ISymptom> {
                    new Symptom("Muskel svaghed",
                    new List<Region> {
                        Region.UpperArms,
                        Region.LowerArms },
                    2,
                    "" )},
                1,
                "",
                ""
                ));
            return diseases;
        }
    }
}
