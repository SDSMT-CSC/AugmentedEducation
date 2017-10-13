#include <iostream>
#include <string>
#include <fbxsdk.h>

using namespace std;


int main()
{
	string fname = "parabola.obj";

	FbxManager *sdkManager = FbxManager::Create();

	FbxIOSettings *io = FbxIOSettings::Create(sdkManager, IOSROOT);
	sdkManager->SetIOSettings(io);

	FbxImporter *importer = FbxImporter::Create(sdkManager, "");

	if (!importer->Initialize(fname.c_str(), -1, sdkManager->GetIOSettings()))
	{
		cout << "Error while loading file" << endl;
		exit(-1);
	}

	FbxScene *scene = FbxScene::Create(sdkManager, "myScene");

	importer->Import(scene);
	importer->Destroy();

	FbxExporter *exporter = FbxExporter::Create(sdkManager, "");
	
	string exportname = "parabola.fbx";
	if (!exporter->Initialize(exportname.c_str(), -1, sdkManager->GetIOSettings()))
	{
		cout << "Error initializing exporter" << endl;
	}

	exporter->Export(scene);
	exporter->Destroy();

	scene->Destroy();


	system("pause");
    return 0;
}

