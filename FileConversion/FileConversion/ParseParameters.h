#pragma once
#include <string>
#include <vector>
using namespace std;

class ParseParameters
{
public:
	ParseParameters(int count, char **args);
	~ParseParameters();

	bool success;
	std::string inputFile;
	std::string outputFile;

	std::string fileExtention;

private:
	void PrintUsage();

	vector<string> arguments;

};
