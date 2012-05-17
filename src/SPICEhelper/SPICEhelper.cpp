#include <io.h>
#include <iostream>
#include <fstream>
#include <regex>
#include <string>
#include <sstream>
#include <time.h>
#include "SpiceUsr.h"

using namespace std;
using namespace std::tr1;

extern "C" __declspec(dllexport) void __stdcall getSPICEVersion(char* version, int* length) 
{
	ConstSpiceChar *versn;
	versn = tkvrsn_c( "TOOLKIT" );

	for(unsigned int i = 0; i < strlen(versn); i++)
	{
		version[i] = versn[i];
	}
	*length = (strlen(version) + 1) * sizeof(char);
}

extern "C" __declspec(dllexport) void __stdcall calculateSunPosition(char* utc, double* x, double* y, double* z)
{
	furnsh_c("../kernels/moon_pa_de421_1900-2050.bpc");
	furnsh_c("../kernels/moon_080317.tf");
	furnsh_c("../kernels/moon_assoc_me.tf");
	furnsh_c("../kernels/pck00009.tpc");
	furnsh_c("../kernels/naif0009.tls");
	furnsh_c("../kernels/de421.bsp");
	
	/*
	SpiceInt frcode;
	SpiceChar frname[80];
	SpiceBoolean found;
    cnmfrm_c("MOON", 80, &frcode, frname, &found);
	
	cout << "Reference frame: " << frname << endl;
	*/

	SpiceDouble et;
	utc2et_c(utc, &et);

	SpiceDouble ptarg[3];
	SpiceDouble lt;
	spkpos_c("SUN", et, "MOON_ME", "NONE", "MOON", ptarg, &lt);

	/*
	cout << "x: " << ptarg[0] << endl;
	cout << "y: " << ptarg[1] << endl;
	cout << "z: " << ptarg[2] << endl;
	*/

	*x = ptarg[0] * 1000;
	*y = ptarg[1] * 1000;
	*z = ptarg[2] * 1000;

	kclear_c();
}