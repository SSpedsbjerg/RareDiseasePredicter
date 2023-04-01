﻿using RareDiseasePredicter.Enums;
using RareDiseasePredicter.Implementations;
using RareDiseasePredicter.Interfaces;

namespace RareDiseasePredicter.Controller {
    static class DatabaseController {

        //TODO: This is just for dummy data and should be connected to a database
        public static async Task<ICollection<ISymptom>> getSymptoms() {
            List<ISymptom> symptoms = new List<ISymptom>();
            symptoms.Add(new Symptom("Headache", new List<Region> { Region.Head }, 0, "Pain in Brain"));
            symptoms.Add(new Symptom("Pain", new List<Region> { Region.Head, Region.Face, Region.Lips, Region.Mouth, Region.Ear, Region.Neck, Region.Shoulders }, 1, "Pain"));
            return symptoms;
            }

        //TODO: This is just for dummy data and should be connected to a database
        public static async Task<ICollection<IDisease>> getDiseases(){
            List<IDisease> diseases = new List<IDisease>();
            diseases.Add(new Disease("Steven Johnson",
                new List<ISymptom> {
                    new Symptom("Pain",
                    new List<Region> {
                        Region.Head,
                        Region.Face,
                        Region.Lips,
                        Region.Mouth,
                        Region.Ear,
                        Region.Neck },
                    1,
                    "Pain is low but is spread over the whole body" )},
                0,
                "Commenly arises from medicine",
                "https://www.nhs.uk/conditions/stevens-johnson-syndrome/"
                ));

            diseases.Add(new Disease("Pompes",
                new List<ISymptom> {
                    new Symptom("Muscle Weakness",
                    new List<Region> {
                        Region.UpperArms,
                        Region.LowerArms },
                    2,
                    "Tolerable Headache" )},
                1,
                "",
                ""
                ));
            return diseases;
        }
    }
}