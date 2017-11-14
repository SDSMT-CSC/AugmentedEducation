#include "AbstractConverter.h"

std::string AbstractConverter::ExtractFileExtention(std::string fileName)
{
	return fileName.substr(fileName.rfind('.'));
}