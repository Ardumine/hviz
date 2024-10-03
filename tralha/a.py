import open3d as o3d
import numpy as np

class Point:
    def __init__(self, radius, angle):
        self.Radius = radius  # cm
        self.Angle = angle  # radian

def loadData():
    # This function should return a list of scans where each scan is a list of Point objects.
    # For demonstration purposes, we'll create some dummy data.
    scans = []
    for _ in range(10):  # Assuming 10 scans
        scan = [Point(np.random.rand() * 100, np.random.rand() * 2 * np.pi) for _ in range(360)]
        scans.append(scan)
    return scans

# Load data from your lidar
data = loadData()

# Convert points to Open3D format
points = []
for scan in data:
    pcd_temp = o3d.geometry.PointCloud()
    pts = []
    for point in scan:
        x = point.Radius * np.cos(point.Angle)
        y = point.Radius * np.sin(point.Angle)
        z = 0  # Assuming a 2D lidar and all points are at the same Z level
        pts.append([x, y, z])
    pcd_temp.points = o3d.utility.Vector3dVector(pts)
    points.append(pcd_temp)

# Visualize the point cloud
for i in range(len(points)):
    o3d.visualization.draw_geometries([points[i]])

# Perform SLAM (this is a simplified example, you might want to use more advanced techniques for real-world applications)
def slam(scans):
    poses = []
    current_pose = np.identity(3)  # Initial pose
    poses.append(current_pose)
    
    # Here you would typically integrate the scans into a map and update the poses using an SLAM algorithm
    # For simplicity, we'll just visualize the initial scan without any mapping or optimization
    
    return poses

poses = slam(data)

# Visualize the final pose (this is highly simplified)
final_pose = np.mean(poses, axis=0)  # Average of all poses for demonstration purposes
mesh_frame = o3d.geometry.TriangleMesh.create_coordinate_frame(size=100)
o3d.visualization.draw_geometries([points[0], mesh_frame])