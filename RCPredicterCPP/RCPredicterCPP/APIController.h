#pragma once
#include <windows.data.json.h>
#include "Disease.h"
#include <random>
#include <curl/curl.h>
#include <nlohmann/json.hpp>
#include <fstream>
#include <iostream>

namespace RareDiseaseCalculator {
	using namespace RareDiseaseCalculator;

	class APIController {
	public:
		APIController(const std::string& url);
		~APIController();

		std::vector<RareDiseaseCalculator::Region*> GetRegions();
		std::vector<RareDiseaseCalculator::Symptom*> GetSymptoms();
		std::vector<RareDiseaseCalculator::Disease*> GetDiseases();

		bool InsertTestData(int nrOfRegions, int nrOfSymptoms, int nrOfDiseases);

	private:
		std::vector<RareDiseaseCalculator::Region*> regions;
		std::vector<RareDiseaseCalculator::Symptom*> symptoms;
		std::vector<RareDiseaseCalculator::Disease*> diseases;
		std::string url = "localhost";
		bool TestMode = false;
		bool CreateRegions();
		bool CreateSymptoms(std::vector<RareDiseaseCalculator::Region*> regions);
		bool CreateDiseases(std::vector<RareDiseaseCalculator::Symptom*> symptoms);
		CURL* curl;
		std::string response;
		std::string regionsPath = "";
		std::string symptomsPath = "";
		std::string diseasesPath = "";
	};
}
