#include "FileConverter.h"

FileConverter::FileConverter()
{
	sdkManager = FbxManager::Create();
	FbxIOSettings *io = FbxIOSettings::Create(sdkManager, IOSROOT);
	sdkManager->SetIOSettings(io);

	sceneFBX = nullptr;
}

FileConverter::~FileConverter()
{
	sdkManager->Destroy();
}

FileConverter::Return FileConverter::LoadFile(std::string filename)
{
	if (sceneFBX != nullptr)
	{
		sceneFBX->Destroy();
		sceneFBX = nullptr;
	}

	FbxImporter *importer = FbxImporter::Create(sdkManager, "");

	if (!importer->Initialize(filename.c_str(), -1, sdkManager->GetIOSettings()))
	{
		return FileConverter::IOError;
	}

	sceneFBX = FbxScene::Create(sdkManager, "");

	importer->Import(sceneFBX);

	importer->Destroy();
	return FileConverter::Success;
}

FileConverter::Return FileConverter::ExportFile(std::string filename)
{
	if (sceneFBX == nullptr)
	{
		return FileConverter::SceneNotLoaded;
	}

	FbxExporter *exporter = FbxExporter::Create(sdkManager, "");

	if (!exporter->Initialize(filename.c_str(), -1, sdkManager->GetIOSettings()))
	{
		return FileConverter::IOError;
	}

	exporter->Export(sceneFBX);

	exporter->Destroy();
	return FileConverter::Success;
}