#pragma once
#include "AbstractConverter.h"
#include <fbxsdk.h>

class FBXConverter : public AbstractConverter
{
public:
	FBXConverter();
	~FBXConverter();

	bool SupportsInputFileType(std::string fileType);
	bool SupportsOutputFileType(std::string fileType);

	Result ConvertFile(std::string inputFileName, std::string outputFileName);

private:
	Result LoadFile(std::string filename);

	Result ExportFile(std::string filename);

	FbxManager *sdkManager;
	FbxScene *sceneFBX;

};

