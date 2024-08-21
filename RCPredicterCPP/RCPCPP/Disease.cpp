#include "Disease.h"


using namespace RareDiseaseCalculator;

Disease::Disease(std::string Name, int ID, std::vector<Symptom*> symptoms) {
	this->Name = Name;
	this->ID = ID;
	this->symptoms = symptoms;
}

Disease::~Disease() {
	delete this;
}

std::string Disease::getName() {
	return Name;
}

int Disease::getID() {
	return ID;
}

std::vector<Symptom*> Disease::getSymptoms() {
	return symptoms;
}

float Disease::getWeight() {
	return Weight;
}

std::vector<float> Disease::symptomWeight() {
	return symptomWeights;
}

Symptom::Symptom(std::string Name, int ID, std::vector<Region*> regions) {
	this->Name = Name;
	this->ID = ID;
	this->regions = regions;
}

RareDiseaseCalculator::Symptom::~Symptom() {
	delete this;
}

std::string Symptom::getName() {
	return Name;
}

int Symptom::getID() {
	return ID;
}

std::vector<Region*> Symptom::getRegions() {
	return regions;
}

Region::Region(std::string Name, int ID) {
	this->Name = Name;
	this->ID = ID;
}

RareDiseaseCalculator::Region::~Region() {
	delete this;
}

std::string Region::getName() {
	return Name;
}

int Region::getID() {
	return ID;
}

void Region::setName(std::string Name) {
	this->Name = Name;
}