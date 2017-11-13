#pragma once
#include "AbstractConverter.h"
#include <set>

class AssimpConverter :
	public AbstractConverter
{
public:
	AssimpConverter();
	~AssimpConverter();

	bool SupportsInputFileType(std::string fileType);
	bool SupportsOutputFileType(std::string fileType);

	Result ConvertFile(std::string inputFileName, std::string outputFileName);
private:
	std::set < std::string> acceptedInputTypes;
	std::set < std::string> acceptedOutputTypes;
};

