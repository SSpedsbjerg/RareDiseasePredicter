#include "APIController.h"

RareDiseaseCalculator::APIController::APIController(const std::string& URL, RareDiseaseCalculator::HttpClient* client) {
	this->URL = URL;
	this->client = client;
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

RareDiseaseCalculator::APIController::~APIController() {

}

std::vector<RareDiseaseCalculator::Region*> RareDiseaseCalculator::APIController::GetRegions() {
	const std::string PATH = "/regions";
	nlohmann::json responseJson;
	std::vector<RareDiseaseCalculator::Region*> regions = std::vector<RareDiseaseCalculator::Region*>();
	responseJson = client->sendGetRequest(URL + PATH);
	for (int i = 0; i < responseJson.size(); i++) {
		regions.push_back(
			new Region(
				std::string(responseJson.at(i).at(0)),
				std::stoi(std::string(responseJson.at(i).at(1)))
			)
		);
	}
	return std::vector<RareDiseaseCalculator::Region*>();
}


bool RareDiseaseCalculator::APIController::CreateRegions() {
	return false;
}


static size_t writeCallback(void* contents, size_t size, size_t nmemb, std::string* output) {
	size_t totalSize = size * nmemb;
	output->append(static_cast<char*>(contents), totalSize);
	return totalSize;
}