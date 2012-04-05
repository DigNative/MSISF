# Moon Surface Illumination Simulation Framework (MSISF) #


The *Moon Surface Illumination Simulation Framework* (MSISF) is a framework of software components for the purpose of generating artificial images (so-called "renderings") of the realistically illuminated Moon's surface. It is the first software known to produce realistic renderings of the entire Moon's surface from virtually every viewpoint using a virtual spacecraft (camera), while simultaneously generating machine-readable information regarding the exactly known parameters for the environmental conditions, such as the local solar illumination angle for every pixel of a rendering showing a point on the Moon's surface.

The MSISF utilizes real topography data (so-called *digital elevation models* &mdash; DEMs) to build a 3D model of the Moon originating in the LDEM products of NASA's *Lunar Reconnaissance Orbiter* (LRO) mission and its *Lunar Orbiter Laser Altimeter* (LOLA) instrument aboard. The Sun's position for the user-given simulation timepoints is calculated using SPICE, a toolkit for astrodynamical calculus offered by NASA's *Navigation and Ancillary Information Facility* (NAIF).

The MSISF has been developed within a [Master's Thesis][Master's Thesis] at the Merseburg University of Applied Sciences by René Schwarz on behalf of the Institute of Space Systems of the German Aerospace Center (DLR). The objective of the MSISF is to support the development of a new optical navigation algorithm for the autonomous terrain-relative navigation and landing of spacecraft for future robotic and human missions to the Moon and other celestial bodies (for more information see the [Master's Thesis][Master's Thesis]). However, the MSISF can be used for other fields of application, too. After the final examination, the MSISF has been released subject to the conditions of the GNU General Public License (GPL), version 2 or later, to the public. It will be developed, maintained and distributed further as open-source software.



## Author ##

> B.Eng. René Schwarz   
> <http://www.rene-schwarz.com>

    
## License ##

The source code of the MSISF software components are subject to the conditions of the GNU General Public License (GPL), version 2 or later, while all documentation/the original [Master's Thesis][Master's Thesis] is subject to the conditions of the [Creative Commons Attribution-NonCommercial 3.0 Unported License (CC BY-NC 3.0 Unported)][CC-BY-NC]. Please note, however, that the MSISF, its documentation and the original [Master's Thesis][Master's Thesis] may also use external software components, images or other elements from third parties; other conditions may apply in individual cases.


   [Master's Thesis]:    http://go.rene-schwarz.com/masters-thesis   "Master's Thesis 'Development of an illumination simulation software for the Moon’s surface: An approach to illumination direction estimation on pictures of solid planetary surfaces with a significant number of craters.' by B.Eng. René Schwarz"
   [CC-BY-NC]:           http://creativecommons.org/licenses/by-nc/3.0/