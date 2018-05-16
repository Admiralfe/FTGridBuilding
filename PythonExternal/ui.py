import numpy as np
import matplotlib.pyplot as plt
from lxml import etree
import sys

grid_size = int(sys.argv[1])
row_index = int(sys.argv[2])
col_index = int(sys.argv[3])
velocity_xml_file = sys.argv[4]
valid_tiles_xml_file = sys.argv[5]

velocity_root = etree.parse(velocity_xml_file).getroot()
valid_tiles_root = etree.parse(valid_tiles_xml_file).getroot()

grid_fig = plt.figure()
grid_ax = grid_fig.add_subplot(111)

valid_tiles_fig = plt.figure()

grid_ax.axis([0, grid_size, 0, grid_size])

for i in range(1, grid_size):
    grid_ax.axhline(i, 0, grid_size, c = 'r')
    grid_ax.axvline(i, 0, grid_size, c = 'r')


for tile in velocity_root:
    xmin = int(tile.get("col"))
    ymin = grid_size - int(tile.get("row")) - 1
    for vel in tile:
        x = xmin + float(vel.get("relX"))
        y = ymin + float(vel.get("relY"))
        vx = float(vel.get("vx"))
        vy = float(vel.get("vy"))
        grid_ax.quiver(x, y, vx, vy, angles = 'xy', scale_units = 'xy', scale = 4, width = 0.001)

no_valid_tiles = len(valid_tiles_root)
valid_tiles_axes = []
for i in range(0, no_valid_tiles):
    valid_tiles_axes.append(valid_tiles_fig.add_subplot(int((no_valid_tiles - no_valid_tiles%2)/2 + 1), 2, i + 1))

for i in range(0, no_valid_tiles):
    ax = valid_tiles_axes[i]
    ax.set_title(i)
    for vel in valid_tiles_root[i]:
        x = float(vel.get("relX"))
        y = float(vel.get("relY"))
        vx = float(vel.get("vx"))
        vy = float(vel.get("vy"))
        ax.quiver(x, y, vx, vy, angles = 'xy', scale_units = 'xy', scale = 4)

def onclick(event):
    print(event.inaxes.get_title())
    sys.exit(0)

cid = valid_tiles_fig.canvas.mpl_connect('button_press_event', onclick)

plt.show()
