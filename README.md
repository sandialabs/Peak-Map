Peak Map is a software tool that assists the user with matching unknown peaks from gamma
spectroscopy measurements with corresponding decay signatures and generating nuclide libraries
to be used in gamma spectroscopy analysis. The system uses nuclear data for the International
Commission on Radiological Protection publication 107: Nuclear Decay Data for Dosimetric
Calculations (ICRP-107) as a nuclear data reference however, the system is deigned in such a way
other data references can be used in future releases. Peak matching is done by matching the energy
of a peak to find nuclides with corresponding gamma line signatures and then provides a score.
The score based on the difference in energy of the peak and line, the photon yield of the, and the
half-life of the nuclide in relation to the time between the reference date and the measurement date.
To further increase the confidence of the selection, the other signature lines of the parent, and of the
daughters in equilibrium, are provided with a line match score that is determined by whether the
lines that should be detected have matching peaks in the list. The signature lines and the matching
nuclide information can then be written to a nuclide library file.
