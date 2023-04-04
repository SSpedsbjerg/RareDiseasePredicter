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

        private static bool CreateTable() {
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

                createQuery = "CREATE TABLE Regions (" +
                    " ID int identity(1,1) not null primary key," +
                    " Name VARCHAR(512)" +
                    ");";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();

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
            const int ID = 0;
            var command = Connection.CreateCommand();
            if(CreateConnection()) {
                foreach(ISymptom symptom in disease.GetSymptoms()) {
                    query = $"SELECT * FROM Symptoms WHERE ID = {symptom.ID};";
                    command.CommandText = query;
                    using (var reader = command.ExecuteReader()) {
                        bool found = false;
                        while(await reader.ReadAsync()) {
                            if (symptom.ID == reader.GetInt32(ID)) {
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
