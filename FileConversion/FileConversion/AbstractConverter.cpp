#include "AbstractConverter.h"

std::string AbstractConverter::ResultString(Result result)
{
	switch (result)
	{
		case Result::Failed:
			return "Failed";
		case Result::FileTypeNotSupported:
			return "File type not supported";
		case Result::IOError:
			return "Input/Output Error";
		case Result::NotInitialized:
			return "Not Initialized";
		case Result::SceneNotLoaded:
			return "File not loaded";
		case Result::Success:
			return "Success";
	}
}

std::string AbstractConverter::ExtractFileExtention(std::string fileName)
{
	return fileName.substr(fileName.rfind('.'));
}