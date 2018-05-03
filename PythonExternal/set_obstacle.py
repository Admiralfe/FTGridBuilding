import numpy as np
import matplotlib.pyplot as plt
import sys

grid_size = int(sys.argv[1])

fig = plt.figure()
ax = fig.add_subplot(111)
ax.axis([0, grid_size, 0, grid_size])

for i in range(1, grid_size):
    ax.axhline(i, 0, grid_size, c = 'r')
    ax.axvline(i, 0, grid_size, c = 'r')

xy_list = plt.ginput(-1)
datastr = ""
for xypair in xy_list:
    datastr = datastr + str(grid_size - 1 - int(xypair[1])) + ',' + str(int(xypair[0])) + ';'
print(datastr)
sys.exit(0)

plt.show()
