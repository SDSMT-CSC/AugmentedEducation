#pragma once

#include <string>

class AbstractConverter
{
public:
	enum Result { Failed = INT_MIN, IOError, SceneNotLoaded, NotInitialized, Success = 1 };

	virtual bool SupportsInputFileType(std::string fileType) = 0;
	virtual bool SupportsOutputFileType(std::string fileType) = 0;

	virtual Result ConvertFile(std::string inputFileName, std::string outputFileName) = 0;
};