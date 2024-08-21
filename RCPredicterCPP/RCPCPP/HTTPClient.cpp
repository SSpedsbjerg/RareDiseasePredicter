#pragma once
#include "HTTPClient.h"



RareDiseaseCalculator::HttpClient::HttpClient() {
    // Initialize libcurl
    curl_global_init(CURL_GLOBAL_ALL);
    curl = curl_easy_init();
}

RareDiseaseCalculator::HttpClient::~HttpClient() {
    // Cleanup libcurl
    if (curl) {
        curl_easy_cleanup(curl);
    }
    curl_global_cleanup();
}

// Function to send an HTTP GET request and retrieve the response
nlohmann::json RareDiseaseCalculator::HttpClient::sendGetRequest(const std::string& url) {
    nlohmann::json responseJson;

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
                std::cerr << "JSON parsing error: " << e.what() << std::endl;
            }
        }
        else {
            std::cerr << "HTTP request error: " << curl_easy_strerror(res) << std::endl;
        }
    }
    else {
        std::cerr << "Failed to initialize libcurl." << std::endl;
    }

    return responseJson;
}

static size_t writeCallback(void* contents, size_t size, size_t nmemb, std::string* output) {
    size_t totalSize = size * nmemb;
    output->append(static_cast<char*>(contents), totalSize);
    return totalSize;
}
