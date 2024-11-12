import tkinter as tk
import numpy as np
import math

def create_curved_path(start, end, num_points=50):
    """
    Generate a curved path between two points.
    
    Parameters:
    start (tuple): (x, y) coordinates of the start point
    end (tuple): (x, y) coordinates of the end point
    num_points (int): Number of points to generate in the path
    
    Returns:
    list: List of (x, y) coordinates representing the curved path
    """
    path = []
    
    for ang in np.linspace(0, 90):  
        x = (math.cos(math.radians(ang)) * (end[0] - start[0])) + (end[0] - start[0]) + start[0]
        y = (math.sin(math.radians(ang)) * (end[1] - start[1])) + start[1]  - (end[1] - start[1])
        path.append((x, y))
    
    return path


class CurvedPathPlanner(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("Curved Path Planner")
        self.geometry("800x600")
        
        # Create a canvas to draw the path
        self.canvas = tk.Canvas(self, bg="white")
        self.canvas.pack(fill="both", expand=True)
        
        # Bind mouse events to canvas
        self.canvas.bind("<Motion>", self.update_end_point)
        self.canvas.bind("<Button-1>", self.set_start_point)
        
        # Initialize start and end points
        self.start_point = (0, 0)
        self.end_point = None
        
        # Draw the initial path
        self.draw_path()
    
    def set_start_point(self, event):
        self.start_point = (event.x, event.y)
        self.draw_path()
    
    def update_end_point(self, event):
        self.end_point = (event.x, event.y)
        self.draw_path()
    
    def draw_path(self):
        self.canvas.delete("path")
        
        if self.end_point is not None:
            path = create_curved_path(self.start_point, self.end_point)
            self.canvas.create_line(path, tags="path")
            
            self.canvas.create_oval(self.start_point[0]-5, self.start_point[1]-5,
                                   self.start_point[0]+5, self.start_point[1]+5,
                                   fill="red", tags="path")
            self.canvas.create_oval(self.end_point[0]-5, self.end_point[1]-5,
                                   self.end_point[0]+5, self.end_point[1]+5,
                                   fill="blue", tags="path")

if __name__ == "__main__":
    app = CurvedPathPlanner()
    app.mainloop()