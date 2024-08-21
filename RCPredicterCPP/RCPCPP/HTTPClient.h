#pragma once
#include <iostream>
#include <string>
#include <curl/curl.h>
#include <nlohmann/json.hpp>

namespace RareDiseaseCalculator {
    class HttpClient {
    public:
        HttpClient();

        ~HttpClient();

        // Function to send an HTTP GET request and retrieve the response
        nlohmann::json sendGetRequest(const std::string& url);

    private:
        CURL* curl;
        std::string response;

        static size_t writeCallback(void* contents, size_t size, size_t nmemb, std::string* output);
    };
}
