#include "AssimpConverter.h"

AssimpConverter::AssimpConverter()
{
	acceptedInputTypes.insert(".fbx");
	acceptedInputTypes.insert(".dae");
	acceptedInputTypes.insert(".glft");
	acceptedInputTypes.insert(".glb");
	acceptedInputTypes.insert(".blend");
	acceptedInputTypes.insert(".3ds");
	acceptedInputTypes.insert(".ase");
	acceptedInputTypes.insert(".obj");
	acceptedInputTypes.insert(".ifc");
	acceptedInputTypes.insert(".xgl");
	acceptedInputTypes.insert(".zgl");
	acceptedInputTypes.insert(".ply");
	acceptedInputTypes.insert(".dxf");
	acceptedInputTypes.insert(".lwo");
	acceptedInputTypes.insert(".lws");
	acceptedInputTypes.insert(".lxo");
	acceptedInputTypes.insert(".stl");
	acceptedInputTypes.insert(".x");
	acceptedInputTypes.insert(".ac");
	acceptedInputTypes.insert(".ms3d");
	acceptedInputTypes.insert(".cob");
	acceptedInputTypes.insert(".scn");

	acceptedOutputTypes.insert(".dae");
	acceptedOutputTypes.insert(".stl");
	acceptedOutputTypes.insert(".obj");
	acceptedOutputTypes.insert(".ply");
}

AssimpConverter::~AssimpConverter()
{
}

bool AssimpConverter::SupportsInputFileType(std::string fileType)
{
	return acceptedInputTypes.count(fileType) > 0;
}

bool AssimpConverter::SupportsOutputFileType(std::string fileType)
{
	return acceptedOutputTypes.count(fileType) > 0;
}

AssimpConverter::Result AssimpConverter::ConvertFile(std::string inputFileName, std::string outputFileName)
{
	return Result::Failed;
}
