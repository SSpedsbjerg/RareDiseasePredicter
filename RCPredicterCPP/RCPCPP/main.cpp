#pragma once
#include <iostream>
#include <windows.data.json.h>
#include "HTTPClient.cpp"
#include "APIController.h"

int main() {
    RareDiseaseCalculator::HttpClient* httpClient = new RareDiseaseCalculator::HttpClient();
    const std::string URL = "https://localhost:57693";
    const std::string PATH = "/regions";

    nlohmann::json jsonResponse = httpClient->sendGetRequest(URL + PATH);

    // Check if the response is not empty
    if (!jsonResponse.empty()) {
        std::cout << "Received JSON response:\n" << jsonResponse.dump(2) << std::endl;
    }
    else {
        std::cerr << "No valid JSON response received." << std::endl;
    }

    RareDiseaseCalculator::APIController* controller = new RareDiseaseCalculator::APIController(URL, httpClient);

    delete httpClient;
    delete controller;
    return 0;
}