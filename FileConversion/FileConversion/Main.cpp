#include <iostream>
#include "FileConverter.h"
#include <string>

const enum ARGUMENTS {FNAME = 1};

void PrintUsage()
{
	std::cout << "FileConversion.exe <file name>" << std::endl;
}

int main(int argc, char ** argv)
{
	if (argc <= 1)
	{
		PrintUsage();
		return -1;
	}

	FileConverter convert;

	std::string inputFileName(argv[FNAME]);
	std::string nameBase = inputFileName.substr(0, inputFileName.rfind('.'));

	FileConverter::Return retVal = convert.LoadFile(inputFileName);
	if (retVal < 0)
	{
		std::cout << "Unalbe to load file parabola.obj" << std::endl;
		return -2;
	}

	std::string outFileName = nameBase + ".fbx";
	retVal = convert.ExportFile(outFileName);
	if (retVal < 0)
	{
		std::cout << "Unable to write file out.fbx" << std::endl;
		return -3;
	}
	
	return 0;
}