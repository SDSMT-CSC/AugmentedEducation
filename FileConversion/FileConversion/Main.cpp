#include <iostream>
#include "FileConverter.h"
#include "ParseParameters.h"
#include <string>

int main(int argc, char ** argv)
{
	ParseParameters parser(argc, argv);
	if (parser.success == false)
	{
		return -1;
	}

	FileConverter convert;

	FileConverter::Return retVal = convert.LoadFile(parser.inputFile);
	if (retVal < 0)
	{
		std::cout << "Unalbe to load: " << parser.inputFile << std::endl;
		return -2;
	}

	retVal = convert.ExportFile(parser.outputFile);
	if (retVal < 0)
	{
		std::cout << "Unable to write to: " << parser.outputFile << std::endl;
		return -3;
	}
	
	return 0;
}