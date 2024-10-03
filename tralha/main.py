import json
import os
from tqdm import tqdm
from dataclasses import dataclass, asdict
from matplotlib import pyplot as plt
import numpy as np
from filterpy.kalman import KalmanFilter
from filterpy.common import Q_discrete_white_noise

@dataclass
class Point:
    Radius: float
    Angle: float
    def __init__(self, radius, angle) -> None:
        self.Radius = radius#cm
        self.Angle = angle#radian



def loadDataa(): 
    fichs = os.listdir("daddos")
    dadosret =[]
    for fich in tqdm(fichs):
        data = json.load(open("daddos/" + fich, "r"))
        dados = []
        for ponto in data["Rays"]:
            a = Point(ponto["Radius"], ponto["Angle"])

            dados.append(a)
        dadosret.append(dados)
        break
    #json.dump(dadosret, open("aa.json", "w+"))
    return dadosret


def loadData():
    # Placeholder for the actual data loading function
    return [
        [Point(10, np.pi/4), Point(20, np.pi/3)],
        [Point(15, np.pi/2), Point(25, 5*np.pi/6)]
    ]

data = loadData()
# Initialize the Kalman filter
dt = 1.0  # Time step (seconds)
dim_x = 3  # State dimension (x, y, theta)
dim_z = len(data[0])  # Measurement dimension (number of lidar points)

kf = KalmanFilter(dim_x=dim_x, dim_z=dim_z)

# Initialize the state vector [x, y, theta]
initial_state = np.zeros((dim_x, 1))
kf.x = initial_state

# State transition matrix A (identity matrix for simplicity)
kf.F = np.eye(dim_x)

# Measurement function H (maps state to measurement space)
def measurement_function(x):
    x_, y_, _ = x
    return np.array([[np.sqrt(x_**2 + y_**2)], [np.arctan2(y_, x_)], [0]])
kf.H = lambda x: measurement_function(x)

# Measurement noise matrix R (depends on lidar precision)
kf.R = np.eye(dim_z) * 5  # Example value, adjust according to your data

# Process noise matrix Q (depends on robot movement model)
Q_scale = 0.1  # Example value, adjust based on motion model
Q = Q_discrete_white_noise(dim=3, dt=dt, var=Q_scale)
kf.Q = Q

# Measurement list to store lidar measurements
measurements = []

# Process each scan in the data
for scan in data:
    z = np.zeros((dim_z, 1))
    for point in scan:
        x = point.Radius * np.cos(point.Angle)
        y = point.Radius * np.sin(point.Angle)
        z[0] = np.sqrt(x**2 + y**2)
        z[1] = np.arctan2(y, x)
        measurements.append(z)

# Convert measurements to a numpy array
measurements = np.array(measurements)

# Run the Kalman filter on the measurements
for i in range(len(measurements)):
    kf.predict()
    a = measurements[i]
    print(a)
    kf.update(a)

# Final state estimate is [x, y, theta]
final_position = kf.x[:2].flatten()
print("Final Position (x, y):", final_position)