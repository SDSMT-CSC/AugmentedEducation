#include "FileConverter.h"

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
	return fbx.ConvertFile(inputFilename, outputFilename);
}