Random walker program for Dynamic Equivalence between the hopping process and the Brownian cage

This program was coded within the manuscript entitled "A dynamical equivalence for the cageing-release colloidal problem", by authors Ivany del Carmen Romero-Sanchez, Leonardo Dagdug, and Erick Sarmiento-Gómez, submitted to Physical Review E, Letters section.

A random walker simulation is performed, with a Gaussian step distribution between consecutive times. The walker can be permanently confined within a cage, or a transition probability can be set. The cage can also perform Brownian motion with a given step distribution (also Gaussian).

The code is developed in VB.NET Framework 4.7, using Microsoft Visual Studio 2026. An implementation of the xoshiro256 PRNM was performed.

Examples: Using default parameters for the random walker (DeltaT = 0.1, DetalXp=0.25, MaxTime=100000, Cage+Pos=1.0), three sets of 100 trajectories were generated, and their MSD was calculated. The first case corresponds to a transition probability of 0.025 (PartJumpProb=0.025), the second to a Brownian cage with a DeltaXCage=0.025, and finally a mixed case (PartJumpProb=0.0046 and DeltaXCage=0.06). The results are shown in the Example Folder. 

For the Mean Squared Displacement calculation, this code can be used: https://github.com/esarmiento-ugto/MSD-calculation/
