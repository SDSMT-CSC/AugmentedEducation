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
	int returnValue = convert.ConvertFile(parser.inputFile, parser.outputFile);
	if (returnValue < 0)
	{
		cout << "Error while converting file " << parser.inputFile << " to " << parser.outputFile << endl;
		cout << AbstractConverter::ResultString((AbstractConverter::Result) returnValue) << endl;
		return returnValue;
	}

	return 0;
}