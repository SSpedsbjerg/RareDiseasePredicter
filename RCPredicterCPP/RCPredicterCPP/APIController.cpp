#include "pch.h"
#include "APIController.h"

RareDiseaseCalculator::APIController::APIController(const std::string& url) {
	this->url = url;
	// Initialize libcurl
	curl_global_init(CURL_GLOBAL_ALL);
	curl = curl_easy_init();
	std::string outputText;
	//The path is determined here for run time optimaztion reasons, removes the possibility of runtime modifiability.
	std::ifstream config("Config.txt");
	
	while (getline(config, outputText)) {
		int found = outputText.find(":");
		if (outputText.substr(0, --found) == std::string("RegionsPath")) {//TODO: Tjek om --found lander det rigtige sted på string array'en
			regionsPath = outputText.substr(outputText.length() - found, found); //TODO: Tjek om found lander det rigtige sted på string array'en
		}
		else if (outputText.substr(0, --found) == std::string("SymptomsPath")) {//
			symptomsPath = outputText.substr(outputText.length() - found, found);//
		}
		else if (outputText.substr(0, --found) == std::string("DiseasePath")) {//
			diseasesPath = outputText.substr(outputText.length() - found, found);//
		}
	}

}

RareDiseaseCalculator::APIController::~APIController(){
	if (curl) {
		curl_easy_cleanup(curl);
	}
	curl_global_cleanup();
}

std::vector<RareDiseaseCalculator::Region*> RareDiseaseCalculator::APIController::GetRegions(){
	nlohmann::json responseJson;
	std::string path = "";
	std::vector<RareDiseaseCalculator::Region*> regions = std::vector<RareDiseaseCalculator::Region*>();
	if (curl) {
		curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
		curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1L);
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, writeCallback);
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response);

		CURLcode res = curl_easy_perform(curl);

		if (res == CURLE_OK) {
			try {
				responseJson = nlohmann::json::parse(response);
			}
			catch (const std::exception& e) {
				std::cerr << "JSON PARSING ERROR: " << e.what() << std::endl;
			}
		}

		for (int i = 0; i < responseJson.size(); i++) {
			regions.push_back(new Region(std::string(responseJson.at(i).at(0)), std::stoi(responseJson.at(i).at(1))))
		}
	}
	return std::vector<RareDiseaseCalculator::Region*>();
}

bool RareDiseaseCalculator::APIController::InsertTestData(int nrOfRegions, int nrOfSymptoms, int nrOfDiseases){
	TestMode = true;
	bool success = false;
	unsigned int seed = 0;
	int MAXVALUE = 5;
	using namespace RareDiseaseCalculator;
	//Creates dummy regions
	for (int i = 0; i < nrOfRegions; i++) {
		std::string name = "Region" + i;
		regions.push_back(new Region(name, i));
	}
	//Creates dummy symptoms with references to regions
	for (int i = 0; i < nrOfSymptoms; i++) {
		std::string name = "Symptom" + i;
		std::srand(seed);
		int RNumbers = (std::rand() % MAXVALUE) + 1;
		int RPos[5];
		std::vector<Region*> regions_;
		for (int j = 0; j < RNumbers; j++) {
			regions_.push_back(regions.at(std::rand() % regions.size()));
		}
		symptoms.push_back(new Symptom(name, i, regions_));
	}
	//Creates dummy diseases with references to symptoms
	for (int i = 0; i < nrOfSymptoms; i++) {
		std::string name = "Disease" + i;
		int RNumbers = (std::rand() % MAXVALUE) + 1;
		int RPos[5];
		std::vector<Symptom*> symptoms_;
		for (int j = 0; j < RNumbers; j++) {
			symptoms_.push_back(symptoms.at(std::rand() % symptoms.size()));
		}
		diseases.push_back(new Disease(name, i, symptoms_));
	}
	return success;
}

bool RareDiseaseCalculator::APIController::CreateRegions(){
	return false;
}


static size_t writeCallback(void* contents, size_t size, size_t nmemb, std::string* output) {
	size_t totalSize = size * nmemb;
	output->append(static_cast<char*>(contents), totalSize);
	return totalSize;
}