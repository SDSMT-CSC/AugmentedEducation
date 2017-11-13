#pragma once

#include <string>
#include <limits>
#include "AbstractConverter.h"
#include "FBXConverter.h"

class FileConverter : public AbstractConverter
{
public:
	FileConverter();
	
	~FileConverter();

	bool SupportsInputFileType(std::string fileType);
	bool SupportsOutputFileType(std::string fileType);

	Result ConvertFile(std::string inputFileName, std::string outputFileName);

private:	
	enum ConversionTool {NotInitialized = -1, FBX = 0, Assimp = 1};
	ConversionTool importTool;
	ConversionTool intermediateTool;
	ConversionTool exportTool;

	FBXConverter fbx;

};