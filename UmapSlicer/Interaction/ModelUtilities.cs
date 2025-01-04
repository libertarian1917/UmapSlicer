using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace UmapSlicer.Interaction
{
    public static class ModelUtilities
    {



        public static Vector3D GetSize(Model3D model)
        {
            return CalculateSize(model);
        }

        public static Vector3D GetSize(ModelVisual3D modelVisual)
        {
            return modelVisual.Content is Model3D model ? CalculateSize(model) : new Vector3D(0, 0, 0);
        }

        public static Vector3D GetSize(GeometryModel3D geometryModel)
        {
            return CalculateSize(geometryModel);
        }

        public static (Point3D minPoint, Point3D maxPoint) GetMinAndMaxPoints(Model3D model)
        {
            Rect3D bounds = model.Bounds;

            Point3D minPoint = new Point3D(bounds.X, bounds.Y, bounds.Z);
            Point3D maxPoint = new Point3D(bounds.X + bounds.SizeX,
                                           bounds.Y + bounds.SizeY,
                                           bounds.Z + bounds.SizeZ);
            return (minPoint, maxPoint);
        }

        // Метод для вычисления размера
        public static Vector3D CalculateSize(Model3D model)
        {
            if (model != null)
            {
                (Point3D minPoint, Point3D maxPoint) = GetMinAndMaxPoints(model);

                var size = new Vector3D(maxPoint.X - minPoint.X, maxPoint.Y - minPoint.Y, maxPoint.Z - minPoint.Z);
                Debug.WriteLine("X: " + size.X + ", Y: " + size.Y + ", Z: " + size.Z);
                return size;
            }
            else throw new ArgumentNullException(nameof(model), "Model was null");
        }

        public static void ResizeObject(Model3D model, double scaleFactor)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "Model cannot be null.");

            if (scaleFactor <= 0)
                throw new ArgumentOutOfRangeException(nameof(scaleFactor), "Scale factor must be greater than 0.");

            Transform3D currentTransform = model.Transform ?? Transform3D.Identity;

            ScaleTransform3D scaleTransform = new ScaleTransform3D(scaleFactor, scaleFactor, scaleFactor);

            // Объединяем текущую трансформацию с новой трансформацией масштаба
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(currentTransform);
            transformGroup.Children.Add(scaleTransform);

            model.Transform = transformGroup;
        }


    }
}
