#include "FBXConverter.h"
#include <set>

FBXConverter::FBXConverter()
{
	sdkManager = FbxManager::Create();
	FbxIOSettings *io = FbxIOSettings::Create(sdkManager, IOSROOT);
	sdkManager->SetIOSettings(io);

	sceneFBX = nullptr;

	acceptedInputTypes.insert(".dae");
	acceptedInputTypes.insert(".obj");
	acceptedInputTypes.insert(".fbx");
	acceptedInputTypes.insert(".dxf");

	acceptedOutputTypes.insert(".dae");
	acceptedOutputTypes.insert(".obj");
	acceptedOutputTypes.insert(".fbx");
	acceptedOutputTypes.insert(".dxf");
}


FBXConverter::~FBXConverter()
{
	sdkManager->Destroy();
}

bool FBXConverter::SupportsInputFileType(std::string fileType)
{
	return acceptedInputTypes.count(fileType) > 0;
}

bool FBXConverter::SupportsOutputFileType(std::string fileType)
{
	return acceptedOutputTypes.count(fileType) > 0;
}

FBXConverter::Result FBXConverter::ConvertFile(std::string inputFileName, std::string outputFileName)
{
	FBXConverter::Result retVal = LoadFile(inputFileName);
	if (retVal < 0)
	{
		return retVal;
	}

	retVal = ExportFile(outputFileName);
	if (retVal < 0)
	{
		return retVal;
	}

	return Result::Success;
}

FBXConverter::Result FBXConverter::ExportFile(std::string filename)
{
	if (sceneFBX == nullptr)
	{
		return FBXConverter::SceneNotLoaded;
	}

	FbxExporter *exporter = FbxExporter::Create(sdkManager, "");

	if (!exporter->Initialize(filename.c_str(), -1, sdkManager->GetIOSettings()))
	{
		return Result::IOError;
	}

	exporter->Export(sceneFBX);

	exporter->Destroy();
	return Result::Success;
}

FBXConverter::Result FBXConverter::LoadFile(std::string filename)
{
	if (sceneFBX != nullptr)
	{
		sceneFBX->Destroy();
		sceneFBX = nullptr;
	}

	FbxImporter *importer = FbxImporter::Create(sdkManager, "");

	if (!importer->Initialize(filename.c_str(), -1, sdkManager->GetIOSettings()))
	{
		return Result::IOError;
	}

	sceneFBX = FbxScene::Create(sdkManager, "");

	importer->Import(sceneFBX);

	importer->Destroy();
	return Result::Success;
}