#include "FileConverter.h"

FileConverter::FileConverter()
{
	sdkManager = FbxManager::Create();
	FbxIOSettings *io = FbxIOSettings::Create(sdkManager, IOSROOT);
	sdkManager->SetIOSettings(io);

	scene = nullptr;
}

FileConverter::~FileConverter()
{
	sdkManager->Destroy();
}

FileConverter::Return FileConverter::LoadFile(std::string filename)
{
	if (scene != nullptr)
	{
		scene->Destroy();
		scene = nullptr;
	}

	FbxImporter *importer = FbxImporter::Create(sdkManager, "");

	if (!importer->Initialize(filename.c_str(), -1, sdkManager->GetIOSettings()))
	{
		return FileConverter::IOError;
	}

	scene = FbxScene::Create(sdkManager, "");

	importer->Import(scene);

	importer->Destroy();
	return FileConverter::Success;
}

FileConverter::Return FileConverter::ExportFile(std::string filename)
{
	if (scene == nullptr)
	{
		return FileConverter::SceneNotLoaded;
	}

	FbxExporter *exporter = FbxExporter::Create(sdkManager, "");

	if (!exporter->Initialize(filename.c_str(), -1, sdkManager->GetIOSettings()))
	{
		return FileConverter::IOError;
	}

	exporter->Export(scene);

	exporter->Destroy();
	return FileConverter::Success;
}