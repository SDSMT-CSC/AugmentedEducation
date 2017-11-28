#pragma once

#include <string>

class AbstractConverter
{
public:
	enum Result { Failed = INT_MIN, IOError, SceneNotLoaded, NotInitialized, FileTypeNotSupported, Success = 1 };

	static std::string ResultString(Result result);

	virtual bool SupportsInputFileType(std::string fileType) = 0;
	virtual bool SupportsOutputFileType(std::string fileType) = 0;

	virtual Result ConvertFile(std::string inputFileName, std::string outputFileName) = 0;

	std::string ExtractFileExtention(std::string fileName);
};