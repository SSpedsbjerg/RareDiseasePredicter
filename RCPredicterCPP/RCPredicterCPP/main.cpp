#include "pch.h" //Find a way to get rid of this
#include "main.h"
#include "HTTPClient.cpp"

int main() {
    HttpClient httpClient;
    std::string url = "http://83.92.23.39:57693/regions";

    nlohmann::json jsonResponse = httpClient.sendGetRequest(url);

    // Check if the response is not empty
    if (!jsonResponse.empty()) {
        std::cout << "Received JSON response:\n" << jsonResponse.dump(2) << std::endl;
    }
    else {
        std::cerr << "No valid JSON response received." << std::endl;
    }

    return 0;
}