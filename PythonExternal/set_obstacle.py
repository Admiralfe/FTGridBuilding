import numpy as np
import matplotlib.pyplot as plt
from matplotlib.patches import Rectangle
from matplotlib.collections import PatchCollection
import sys
import os

f = open(os.devnull, 'w')
sys.stderr = f

grid_size = int(sys.argv[1])
obstacle_str = sys.argv[2]
eligible_obstacles_str = sys.argv[3]

#grid_size = 6
#obstacle_str = '1,1;3,4'
#eligible_obstacles_str = '2,2;1,4'

fig = plt.figure()
ax = fig.add_subplot(111)
ax.axis([0, grid_size, 0, grid_size])

plt.hold(True)

for i in range(1, grid_size):
    ax.axhline(i, 0, grid_size, c = 'r')
    ax.axvline(i, 0, grid_size, c = 'r')

obstacles = [] #using (row, col) not (x, y)
if(obstacle_str != ''):
    for rowcol_str in obstacle_str.split(';'):
        obstacles.append(Rectangle((int(rowcol_str.split(',')[1]), grid_size - 1 - int(rowcol_str.split(',')[0])), 1, 1))
ax.add_collection(PatchCollection(obstacles, facecolor='k'))

eligible_obstacles = [] #using (row, col) not (x, y)
if(eligible_obstacles_str != ''):
    for rowcol_str in eligible_obstacles_str.split(';'):
        eligible_obstacles.append(Rectangle((int(rowcol_str.split(',')[1]), grid_size - 1 - int(rowcol_str.split(',')[0])), 1, 1))
ax.add_collection(PatchCollection(eligible_obstacles, facecolor='b'))

xy = plt.ginput(1)
print(xy)
datastr = str(grid_size - 1 - int(xy[0][1])) + ',' + str(int(xy[0][0]))
print(datastr)
sys.exit(0)

plt.show()
