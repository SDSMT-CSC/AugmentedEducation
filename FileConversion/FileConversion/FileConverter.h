#pragma once

#include <string>
#include <fbxsdk.h>
#include <limits>

class FileConverter
{
public:
	enum Return {Failed = INT_MIN, IOError, SceneNotLoaded, NotInitialized, Success = 1};

	FileConverter();
	
	~FileConverter();

	Return LoadFile(std::string filename);

	Return ExportFile(std::string filename);

private:
	FbxManager *sdkManager;
	FbxScene *scene;
};