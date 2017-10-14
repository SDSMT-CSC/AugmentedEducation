#include <iostream>
#include "FileConverter.h"
#include <string>

int main()
{
	FileConverter convert;

	std::string filename = "parabola.obj";

	FileConverter::Return retVal = convert.LoadFile(filename);
	if (retVal < 0)
	{
		std::cout << "Unalbe to load file parabola.obj" << std::endl;
		exit(-1);
	}

	std::string outFileName = "out.fbx";
	retVal = convert.ExportFile(outFileName);
	if (retVal < 0)
	{
		std::cout << "Unable to write file out.fbx" << std::endl;
		exit(-2);
	}
	
	return 0;
}