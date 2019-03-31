import numpy as np
import matplotlib.pyplot as plt
 
# set width of bar
barWidth = 0.2

#number of survey for each category
numSurveys=[3,4,5]
real1 = [90,82,100]
real2 = [92,98,81]
real3 = [68,90,80]
real4 = [0,98,88]
real5 = [0,0,97]

# set height of bar
 
# Set position of bar on X axis
r1 = np.arange(len(real1))
r2 = [x + barWidth for x in r1]
r3 = [x + barWidth for x in r2]
r4 = [x + barWidth for x in r3]
r5 = [x + barWidth for x in r4]

# Make the plot
plt.bar(r1, real1, color='#557f2d', width=barWidth, edgecolor='white', label='real exploration percentage')
plt.bar(r2, real2, color='#557f2d', width=barWidth, edgecolor='white')
plt.bar(r3, real3, color='#557f2d', width=barWidth, edgecolor='white')
plt.bar(r4, real4, color='#557f2d', width=barWidth, edgecolor='white')
plt.bar(r5, real5, color='#557f2d', width=barWidth, edgecolor='white')
 
# Add xticks on the middle of the group bars
plt.xticks([0.2,1.3,2.4], ['<50%', '50-75%', '75-100%'])
 
# Create legend & Show graphic
plt.legend()
plt.show()






