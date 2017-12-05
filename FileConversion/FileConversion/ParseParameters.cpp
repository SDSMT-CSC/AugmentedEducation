#include "ParseParameters.h"
#include <string>
#include <iostream>

void ParseParameters::PrintUsage()
{
	std::cout << "FileConversion.exe -i <Path to Input File> -t <.extention> [-o <Path to Output File>]" << std::endl;
	std::cout << "FileConversion.exe -i <Path to Input File> -t <.extention> [-odir <Path to Output File Directory>]" << std::endl;
}


ParseParameters::ParseParameters(int count, char **args)
{
	for (int i = 0; i < count; i++)
	{
		string temp = args[i];
		arguments.push_back(temp);
	}

	this->inputFile = "";
	this->outputFile = "";
	this->fileExtention = "";
	this->success = false;

	std::string outDir = "";

	unsigned int i = 1;
	while (i < arguments.size())
	{
		std::string arg = arguments[i];
		if (arg == "-i")
		{
			i++;
			if (i >= arguments.size())
			{
				std::cout << "Please provide an input file path." << std::endl;
				PrintUsage();
				return;
			}

			this->inputFile = arguments[i];
		}
		else if (arg == "-o")
		{
			i++;
			if (i >= arguments.size())
			{
				std::cout << "Please provide a path to export to." << std::endl;
				PrintUsage();
				return;
			}

			this->outputFile = arguments[i];
		}
		else if (arg == "-odir")
		{
			i++;
			if (this->outputFile == "")
			{
				if (i >= arguments.size())
				{
					std::cout << "Please provide a directory path to write to." << std::endl;
					PrintUsage();
					return;
				}

				outDir = arguments[i];
			}
		}
		else if (arg == "-t")
		{
			i++;
			if (i >= arguments.size())
			{
				std::cout << "Please provide a file extention." << std::endl;
				PrintUsage();
				return;
			}

			fileExtention = arguments[i];
		}

		i++;
	}

	if (inputFile == "")
	{
		std::cout << "Input file path required." << std::endl;
		PrintUsage();
		return;
	}

	std::string nameBase;
	if (outputFile == "")
	{
		if (fileExtention == "")
		{
			cout << "File extention needed." << endl;
		}

		if (fileExtention.find('.') == string::npos)
		{
			fileExtention = '.' + fileExtention;
		}

		if (outDir == "")
		{
			outDir = ".\\";
		}
		else if (outDir[outDir.size() - 1] != '\\')
		{
			outDir += "\\";
		}

		outputFile = outDir;
		int start = inputFile.rfind('\\') + 1;
		outputFile += inputFile.substr(start, inputFile.rfind('.') - start) + fileExtention;
	}

	success = true;
}


ParseParameters::~ParseParameters()
{
}
