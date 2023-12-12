#pragma once
#include <string>
#include <vector>

namespace RareDiseaseCalculator {
	class Region {
	public:
		Region(std::string Name, int ID);
		~Region();

		std::string getName();
		int getID();
	private:
		void setName(std::string Name);
		std::string Name = "";
		int ID = -1;

	};

	class Symptom {
	public:
		Symptom(std::string Name, int ID, std::vector<Region*> regions);
		~Symptom();

		std::string getName();
		int getID();
		std::vector<Region*> getRegions();
	private:
		std::string Name = "";
		int ID = -1;
		std::vector<Region*> regions;

	};

	class Disease {
	public:
		Disease(std::string Name, int ID, std::vector<Symptom*> symptoms);
		~Disease();

		std::string getName();
		int getID();
		std::vector<RareDiseaseCalculator::Symptom*> getSymptoms();
		float getWeight();
		std::vector<float> symptomWeight();
		std::vector<RareDiseaseCalculator::Symptom*> symptoms;
		std::vector<float> symptomWeights;
	private:
		std::string Name = "";
		int ID = -1;
		float Weight = 0.0f;
	};
}
