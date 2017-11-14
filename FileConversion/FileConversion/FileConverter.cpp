#include "FileConverter.h"
#include <iostream>

FileConverter::FileConverter()
{
	importTool = ConversionTool::NotInitialized;
	intermediateTool = ConversionTool::NotInitialized;
	exportTool = ConversionTool::NotInitialized;
}

FileConverter::~FileConverter()
{

}

bool FileConverter::SupportsInputFileType(std::string fileType)
{
	return false;
}

bool FileConverter::SupportsOutputFileType(std::string fileType)
{
	return false;
}

FileConverter::Result FileConverter::ConvertFile(std::string inputFilename, std::string outputFilename)
{
	std::string inputFileType = this->ExtractFileExtention(inputFilename);
	std::string outputFileType = this->ExtractFileExtention(outputFilename);

	if (assimp.SupportsInputFileType(inputFileType) && assimp.SupportsOutputFileType(outputFileType))
	{
		return assimp.ConvertFile(inputFilename, outputFilename);
	}
	else if (fbx.SupportsInputFileType(inputFileType) && fbx.SupportsOutputFileType(outputFileType))
	{
		return fbx.ConvertFile(inputFilename, outputFilename);
	}
	else if (assimp.SupportsInputFileType(inputFileType) && fbx.SupportsOutputFileType(outputFileType))
	{
		std::string tempFilename = inputFilename.substr(0, inputFilename.rfind("."));
		tempFilename = tempFilename + ".dae";

		Result converionResult;
		converionResult = assimp.ConvertFile(inputFilename, tempFilename);

		if (converionResult < 0)
			return converionResult;

		return fbx.ConvertFile(tempFilename, outputFilename);
	}
}