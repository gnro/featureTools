//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Controls;
using System;
using System.Data;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.EditorExt;

namespace featureTools
{
    /// <summary>Proporciona herramientas básicas para trabajar con Capas</summary>
    /// <remarks>Realiza operaciones de consulta y cambio de propiedades de las capas en ArcMap</remarks>
    public class feature
    {
        /// <param name="layerName"> Nombre de la capa del elemento a seleccionar.</param>
        /// <param name="pgeometry"> Visualizacion activa.</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <param name="layer">Layer a editar</param>
        /// <param name="elObjeto">Objeto con los datos (nombre del campo,tipo de valor,valor).</param>
        /// <returns>Retorna el fid de tipo entero.</returns>
        public static int createFeature(string layerName, IGeometry pgeometry, IMxDocument pMxDoc, ILayer layer, string[,] elObjeto =null)
        {
            try
            {
                IFeatureClass pFeatureClass = returnFeatureClassByName(pMxDoc, layerName);
                IActiveView activeView = pMxDoc.ActivatedView;
                IFeature pFeature = pFeatureClass.CreateFeature();
                pFeature.Shape = pgeometry;
                if(elObjeto != null)
                    insertDatoS(pFeatureClass,ref pFeature, elObjeto);
                pFeature.Store();
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, layer, null);
                return pFeature.OID;
            }
            catch (System.Exception ex)
            {
                throw new ArgumentException("createFeature\n Error: " + ex.Message + "\n" + ex.StackTrace );
                return -1;
            }
        }
        /// <summary>Retorna el número de elementos seleccionados según una condición</summary>
        /// <param name="condicion">Condición para realizar la selección</param>
        /// <param name="Layer">Layer de origen de valores.</param>
        /// <returns> Devuelve un entero.</returns>
        public static int cuentaSeleccion(string condicion, ILayer Layer)
        {
            ISelectionSet pSelSet;
            try {
                pSelSet = seleccionByAttributeQuery(condicion, Layer).SelectionSet;
                return pSelSet.Count;
            }
            catch (System.Exception ex) {
                throw ex;
            }
        }
        /// <summary>Retorna un true o false en base a la existencia del nombre del layer</summary>
        /// <param name="layer">Nombre del layer a buscar.</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <returns>Devuelve boolean.</returns>
        public static bool existeono(IMxDocument pMxDoc, string layer)
        {
            ILayer capa = returnLayerByName(pMxDoc, layer);
            if (capa == null)
                return false;
            else
                return true;
        }
        /// <summary>Ejecuta un comando dentro del entorno de arcmap</summary>
        /// <param name="application">Entorno de arcmap</param>
        /// <param name="commandName">Comando a ejecutar</param>
        public static void FindCommandAndExecute(ESRI.ArcGIS.Framework.IApplication application, string commandName) {
            try {
                ESRI.ArcGIS.Framework.ICommandBars commandBars = application.Document.CommandBars;
                ESRI.ArcGIS.esriSystem.UID uid = new ESRI.ArcGIS.esriSystem.UIDClass();
                uid.Value = commandName; // Example: "esriFramework.HelpContentsCommand" or "{D74B2F25-AC90-11D2-87F8-0000F8751720}"
                ESRI.ArcGIS.Framework.ICommandItem commandItem = commandBars.Find(uid, false, false);
                if (commandItem != null)
                 commandItem.Execute();
            }catch (System.Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>Retorna un IGroupLayer en base al nombre</summary>
        /// <param name="layer">Nombre del layer o grupo a buscar</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <returns>Devuelve un IGroupLayer.</returns>
        protected static IGroupLayer getGrpLayerByName(IMxDocument pMxDoc, string layer)
        {
            IMap pMap = pMxDoc.FocusMap;
            IEnumLayer pLayers = pMap.Layers;
            ILayer pLayer;
            IGroupLayer pGroupLayer;
            pGroupLayer = null;
            pMap = pMxDoc.FocusMap;
            pLayers = pMap.Layers;
            for (int i = 0; i < pMap.LayerCount; i++)
                if (pMap.get_Layer(i).Name == layer) {
                    pLayer = pMap.get_Layer(i);
                    if (pLayer is IGroupLayer) {
                        pGroupLayer = (IGroupLayer)pLayer;
                        return pGroupLayer;
                    }
                }
            return pGroupLayer;
        }
        /// <summary> Cambia la propiedad de visibilidad / enciende o apaga de un grupo de capas </summary>
        /// <param name="nomGroupLayer">Nombre del Grouplayer a buscar.</param>
        /// <param name="bandera">Valor booleano de visible</param>
        /// <param name="pMxDoc"> ArcMap.Document.</param>
        public static void groupLayerVisible(IMxDocument pMxDoc, string nomGroupLayer, bool bandera)
        {
            IGroupLayer pLayer = default(IGroupLayer);
            pLayer = getGrpLayerByName(pMxDoc, nomGroupLayer);
            pLayer.Visible = bandera;
        }
        /// <summary> Retorna un IFeatureCursor </summary>
        /// <param name="layerName"> Nombre de la capa del elemento a seleccionar.</param>
        /// <param name="activeView"> Visualizacion activa.</param>
        /// <param name="envelope"> IEnvelope.</param>
        /// <param name="eltiporelacion"> El tipo de relacion.</param>
        /// <returns>Retorna un IFeatureCursor.</returns>
        public static IFeatureCursor selectedfeatures(string layerName, IActiveView activeView, IEnvelope envelope, esriSpatialRelEnum eltiporelacion)
        {
            return selectedFeatureCursor(layerName, activeView, envelope, eltiporelacion);
        }
        /// <summary> Retorna un IFeatureCursor </summary>
        /// <param name="layerName"> Nombre de la capa del elemento a seleccionar.</param>
        /// <param name="activeView"> Visualizacion activa.</param>
        /// <param name="envelope"> IEnvelope.</param>
        /// <param name="eltiporelacion"> El tipo de relacion.</param>
        /// <returns>Retorna un IFeatureCursor.</returns>
        private static IFeatureCursor selectedFeatureCursor(string layerName, IActiveView activeView, IEnvelope envelope, esriSpatialRelEnum eltiporelacion)
        {
            try{
                IMap pMap = activeView.FocusMap;
                ILayer layer = returnLayerByName(null, layerName, activeView, activeView.FocusMap);
                IFeatureLayer pFeatureLayer = (IFeatureLayer)layer;
                IFeatureClass featureClass = pFeatureLayer.FeatureClass;
                System.String shapeFieldName = featureClass.ShapeFieldName;
                // Create a new spatial filter and use the new envelope as the geometry
                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.Geometry = envelope;
                spatialFilter.SpatialRel = (esriSpatialRelEnum)eltiporelacion;
                //spatialFilter.OutputSpatialReference(shapeFieldName) = map.SpatialReference;
                spatialFilter.set_OutputSpatialReference(shapeFieldName, pMap.SpatialReference);
                spatialFilter.GeometryField = shapeFieldName;
                IFeatureCursor featureCursor = featureClass.Search(spatialFilter, false);
                //  activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                return featureCursor;
           }
           catch (System.Exception ex)
            {
                throw ex;
           }
        }
        /// <summary> Permiete seleccionar un elemento por un par de coordenas X,Y</summary>
        /// <param name="x"> coordenada x.</param>
        /// <param name="y"> coordenada y.</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        public static IEnvelope selectByPoint(int x, int y, IActiveView ActiveView)
        {
            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = ActiveView.ScreenDisplay;
            ESRI.ArcGIS.Display.IDisplayTransformation displayTransformation = screenDisplay.DisplayTransformation;
            ESRI.ArcGIS.Geometry.Point pnt = new ESRI.ArcGIS.Geometry.Point();
            pnt = (Point)displayTransformation.ToMapPoint(x, y);
            IEnvelope envelope = pnt.Envelope;
            return envelope;
        }
        /// <summary> Retorna las coordenadas de un punto </summary>
        /// <param name="x"> coordenada x.</param>
        /// <param name="y"> coordenada y.</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        /// <returns>Retorna una arreglo de doubles[2].</returns>
        public static double[] intToGeografic(int x, int y, IActiveView ActiveView)
        {
            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = ActiveView.ScreenDisplay;
            ESRI.ArcGIS.Display.IDisplayTransformation displayTransformation = screenDisplay.DisplayTransformation;
            ESRI.ArcGIS.Geometry.Point pnt = new ESRI.ArcGIS.Geometry.Point();
            pnt = (ESRI.ArcGIS.Geometry.Point)displayTransformation.ToMapPoint(x, y);
            double[] c = { float.Parse(pnt.X.ToString()), float.Parse(pnt.Y.ToString()) };
            return c;
        }
        /// <summary> Retorna las coordenadas de un punto </summary>
        /// <param name="x"> coordenada x.</param>
        /// <param name="y"> coordenada y.</param>
        /// <param name="lacapa"> Nombre de la capa del elemento a seleccionar</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        /// <returns>Retorna una arreglo de doubles [2].</returns>
        public static double[] intToCentroide(int x, int y, string lacapa, IActiveView ActiveView)
        {
            IFeature myfeature = selectPoint(x, y, lacapa, ActiveView);
            return ifeatureCentroide(myfeature);
        }
        /// <summary> Retorna las coordenadas de un punto </summary>
        /// <param name="myfeature"> El IFeature del elemento a seleccionar</param>
        /// <returns>Retorna una arreglo de doubles [2].</returns>
        public static double[] ifeatureCentroide(IFeature myfeature)
        {
            IPoint centerPoint = new ESRI.ArcGIS.Geometry.Point();
            IArea area = myfeature.Shape as IArea;
            area.QueryCentroid(centerPoint);
            double[] c = { centerPoint.X, centerPoint.Y };
            return c;
        }

        /// <summary> Retorna las coordenadas de un punto geografico en pixeles</summary>
        /// <param name="mapPoint"> El IFeature del elemento a seleccionar</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        /// <returns>Retorna un arreglo de enteros.</returns>
        public static int[] pointToMap(IPoint mapPoint, IActiveView ActiveView)
        {
            if (mapPoint == null || mapPoint.IsEmpty || ActiveView == null)
            {
                return null;
            }
            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = ActiveView.ScreenDisplay;
            ESRI.ArcGIS.Display.IDisplayTransformation displayTransformation = screenDisplay.DisplayTransformation;
            int[] c = { 0, 0 };
            displayTransformation.FromMapPoint(mapPoint, out c[0], out c[1]);
            return c;
        }
        /// <summary> Permiete seleccionar un elemento de tipo punto </summary>
        /// <param name="x"> coordenada x.</param>
        /// <param name="y"> coordenada y.</param>
        /// <param name="lacapa"> Nombre de la capa del elemento a seleccionar</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        /// <returns>Retorna un IFeature.</returns>
        public static IFeature selectPoint(int x, int y, string lacapa, IActiveView ActiveView)
        {
            IFeature myfeature;
            IEnvelope envelope = selectByPoint(x, y, ActiveView);
            envelope.Expand(0.1, 0.1, false);
            myfeature = selectedfeature(lacapa, ActiveView, envelope, esriSpatialRelEnum.esriSpatialRelWithin);
            ActiveView.Refresh();
            return myfeature;
        }
        /// <summary> Permiete seleccionar un elemento de tipo poligono </summary>
        /// <param name="x"> coordenada x.</param>
        /// <param name="y"> coordenada y.</param>
        /// <param name="lacapa"> Nombre de la capa del elemento a seleccionar</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        /// <returns>Retorna un IFeature.</returns>
        public static IFeature selectPolygon(int x, int y, string lacapa, IActiveView ActiveView)
        {
            IFeature myfeature;
            IEnvelope envelope = selectByPoint(x, y,  ActiveView);
            envelope.Expand(0.05, 0.05, false);
            myfeature = selectedfeature(lacapa, ActiveView, envelope, esriSpatialRelEnum.esriSpatialRelWithin);
            return myfeature;
        }
        /// <summary> Permiete seleccionar un elemento de tipo linea </summary>
        /// <param name="x"> coordenada x.</param>
        /// <param name="y"> coordenada y.</param>
        /// <param name="lacapa"> Nombre de la capa del elemento a seleccionar</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        /// <returns>Retorna un IFeature.</returns>
        public static IFeature selectLine(int x, int y, string lacapa, IActiveView ActiveView)
        {
            IFeature myfeature;
            IEnvelope envelope = selectByPoint(x, y,  ActiveView);
            envelope.Expand(1, 1, false);
            myfeature = selectedfeature(lacapa, ActiveView, envelope, esriSpatialRelEnum.esriSpatialRelIntersects);
            return myfeature;
        }
        /// <summary> Retorna un IFeatureCursor en base a un nombre de layer</summary>
        /// <param name="layerName"> Nombre de la capa del elemento a seleccionar.</param>
        /// <param name="activeView"> Visualizacion activa.</param>
        /// <param name="envelope"> IEnvelope.</param>
        /// <param name="eltiporelacion"> El tipo de relacion.</param>
        /// <returns>Retorna un IFeature.</returns>
        public static IFeature selectedfeature(string layerName, IActiveView activeView, IEnvelope envelope, esriSpatialRelEnum eltiporelacion)
        {
            IFeatureCursor featureCursor = null;
            featureCursor =selectedFeatureCursor(layerName, activeView, envelope, eltiporelacion);
            IFeature feature = featureCursor.NextFeature();
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            featureCursor = null;
            //System.Runtime.InteropServices.Marshal.FinalReleaseComObject(featureCursor);
            return feature;
        }
        /// <summary> Cambia la propiedad de visibilidad / enciende o apaga la capa </summary>
        /// <param name="nomLayer"> nombre del layer a buscar.</param>
        /// <param name="bandera"> valor booleano de visible</param>
        /// <param name="pMxDoc"> ArcMap.Document.</param>
        public static void layerVisible(IMxDocument pMxDoc, string nomLayer, bool bandera)
        {
            ILayer pLayer = default(ILayer);
            pLayer = returnLayerByName(pMxDoc, nomLayer);
            pLayer.Visible = bandera;
        }
        /// <summary>Retorna una layer en base al nombre</summary>
        /// <param name="layer">Nombre del layer a buscar.</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <param name="activeView"> Visualizacion activa.</param>
        /// <param name="pMap"> Mapa activado.</param>
        /// <returns>Devuelve un ILayer.</returns>
        public static ILayer returnLayerByName(IMxDocument pMxDoc, string layer, IActiveView activeView = null, IMap pMap=null)
        {try{
            if (pMap ==null)
                pMap = pMxDoc.FocusMap;
            if (activeView!=null)
                pMap = activeView.FocusMap;
            IEnumLayer pLayers = pMap.Layers;
            ILayer pLayer = pLayers.Next();
            while (!(pLayer == null)) {
                if (pLayer.Name == layer)
                    return pLayer;
                pLayer = pLayers.Next();
            }
            return null; }
        catch (System.Exception ex) { throw new ArgumentException("returnLayerByName \n Error: " + ex.Message + "\n" + ex.StackTrace); throw ex; }
        }
        /// <summary>Retorna un IFeatureLayer2 en base al nombre</summary>
        /// <param name="layer">Nombre del layer a buscar.</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <returns>Devuelve un IFeatureLayer2.</returns>
        public static IFeatureLayer2 returnFeatureLayerByName(IMxDocument pMxDoc, string layer)
        {
            IMap pMap = pMxDoc.FocusMap;
            IEnumLayer pLayers = pMap.Layers;
            ILayer pLayer = default(ILayer);
            IFeatureLayer2 pFL = default(IFeatureLayer2);
            pLayer = pLayers.Next();
            while (!(pLayer == null)){
                if (pLayer is IFeatureLayer2)
                    if (pLayer.Name == layer){
                        pFL = pLayer as IFeatureLayer2;
                        return pFL;
                    }
                pLayer = pLayers.Next();
            }
            return null;
        }
        /// <summary>Retorna un IFeatureClass en base al nombre</summary>
        /// <param name="layer">Nombre del layer a buscar.</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <returns>Devuelve un IFeatureClass.</returns>
        public static IFeatureClass returnFeatureClassByName(IMxDocument pMxDoc, string layer){
            return  returnFeatureLayerByName(pMxDoc, layer).FeatureClass;
        }
        /// <summary>Retorna un ArrayList de los nombres en un Feature Class</summary>
        /// <param name="fc">FeatureClass a buscar.</param>
        /// <param name="ft">Tipo del campo (opcional)</param>
        /// <returns>Devuelve un ArrayList de valores.</returns>
        public static ArrayList returnFieldNames(IFeatureClass fc, esriFieldType ft = esriFieldType.esriFieldTypeXML)
        {
            ArrayList col = new ArrayList();
            IFields2 pFields = (IFields2)fc.Fields;
            IField2 pField = default(IField2);
            for (int i = 0; i < pFields.FieldCount; i++) {
                pField = (IField2)pFields.Field[i];
                if (ft == esriFieldType.esriFieldTypeXML)
                    col.Add(pField.Name);
                else if (ft == pField.Type)
                    col.Add(pField.Name);
            }
            return col;
        }
        /// <summary>Retorna un ArrayList de valores de un campo en un Feature Class</summary>
        /// <param name="fc">Nombre del Feature a buscar.</param>
        /// <param name="fld">Nombre del campo</param>
        /// <returns>Devuelve un ArrayList de valores.</returns>
        public static ArrayList returnFieldData(IFeatureClass fc, String fld)
        {
            int iFldIndex = fc.Fields.FindField(fld);
            IFeatureCursor pFCursor = fc.Search(null, false);
            IFeature pFeature;
            ArrayList col = new ArrayList();
            pFeature = pFCursor.NextFeature();
            while (!(pFeature == null)){
                col.Add(pFeature.get_Value(iFldIndex));
                pFeature = pFCursor.NextFeature();
            }
            pFCursor = null;
            return col;
        }
        /// <summary>Retorna un ArrayList de valores de un campo en una capa</summary>
        /// <param name="layer">Nombre del layer a buscar.</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <param name="fld">Nombre del campo</param>
        /// <returns>Devuelve un ArrayList de valores.</returns>
        public static ArrayList returnFieldDataSelection(IMxDocument pMxDoc, string layer, String fld)
        {
            IFeatureLayer featureLayer = returnFeatureLayerByName(pMxDoc, layer) as IFeatureLayer;
            if (featureLayer == null)
                return null;
            IFeatureClass fc = returnFeatureClassByName(pMxDoc, layer);
            IFeatureSelection fSel = (IFeatureSelection)featureLayer;
            ISelectionSet selSet = (ISelectionSet)fSel.SelectionSet;
            ICursor cursor = null;
            selSet.Search(null, false, out cursor);
            IFeatureCursor pFCursor = cursor as IFeatureCursor;
            cursor = null;
            IFeature pFeature;
            int iFldIndex = fc.Fields.FindField(fld);
            ArrayList col = new ArrayList();
            pFeature = pFCursor.NextFeature();
            while (!(pFeature == null))
            {
                col.Add(pFeature.get_Value(iFldIndex));
                pFeature = pFCursor.NextFeature();
            }
            return col;
        }
        /// <summary>Retorna un string el valor de un campo de un Feature Class</summary>
        /// <param name="fc">Nombre del Feature a buscar.</param>
        /// <param name="fld">Nombre del campo</param>
        /// <returns>Devuelve un string del valor.</returns>
        public static string returnFirstFieldData(IFeatureClass fc, String fld){
        try{
            int iFldIndex = fc.Fields.FindField(fld);
            IFeatureCursor pFCursor = fc.Search(null, false);
            IFeature pFeature;
            string col = " ";
            pFeature = pFCursor.NextFeature();
            if (!(pFeature == null))
                col = pFeature.get_Value(iFldIndex).ToString();
            pFCursor = null;
            return col; }
            catch (System.Exception ex)
            {
                throw new ArgumentException("returnFirstFieldData \n Error: " + ex.Message + "\n" + ex.StackTrace);
                return "-1";
            }
        }
        /// <summary>Retorna un ArrayList de valores de un campo en un Feature Class</summary>
        /// <param name="fc">Feature class en el que buscar.</param>
        /// <param name="row">Nombres de los campos</param>
        /// <returns>Devuelve un ArrayList de valores.</returns>
        public static DataTable returnData(IFeatureClass fc, ArrayList row)
        {
            DataTable dt = new DataTable();
            string n;
            foreach (var c in row)
                dt.Columns.Add(c.ToString());
            IFeatureCursor updateCursor = fc.Search(null, false);
            IFeature feature = null;
            while ((feature = updateCursor.NextFeature()) != null)
            {
                int i = 0;
                System.Object[] O = new System.Object[row.Count];
                foreach (var c in row)
                {
                    n = c.ToString();
                    O[i] = feature.get_Value(fc.FindField(n)).ToString();
                    i++;
                }
                dt.Rows.Add(O);
            }
            updateCursor = null;
            return dt;
        }
        /// <summary>Retorna un ArrayList en base a la geometria del Layer</summary>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <param name="FT">Geometria del layer a buscar.</param>
        /// <returns>Devuelve un ArrayList.</returns>
        public static ArrayList returnFeatureLayers(IMxDocument pMxDoc, esriGeometryType FT = esriGeometryType.esriGeometryAny)
        {
            IMap pMap = pMxDoc.FocusMap;
            IEnumLayer pLayers = pMap.Layers;
            ILayer pLayer = default(ILayer);
            IFeatureLayer2 pFLayer = default(IFeatureLayer2);
            ArrayList col = new ArrayList();
            pLayer = pLayers.Next();
            while (!(pLayer == null)) {
                if (pLayer is IFeatureLayer) {
                    pFLayer = pLayer as IFeatureLayer2;
                    if (FT == esriGeometryType.esriGeometryAny)
                        col.Add(pLayer.Name);
                    else if (pFLayer.ShapeType == FT)
                        col.Add(pLayer.Name);
                }
                pLayer = pLayers.Next();
            }
            return col;
        }
        /// <summary>Retorna un List en base a la geometria del Layer</summary>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <param name="FT">Geometria del layer a buscar.</param>
        /// <returns>Devuelve un ArrayList.</returns>
        public static List<string> returnAllLayersName(IMxDocument pMxDoc, esriGeometryType FT = esriGeometryType.esriGeometryAny)
        {
            IMap pMap = pMxDoc.FocusMap;
            IEnumLayer pLayers = pMap.Layers;
            ILayer pLayer = default(ILayer);
            IFeatureLayer2 pFLayer = default(IFeatureLayer2);
            List<string> col = new List<string>();
            pLayer = pLayers.Next();
            while (!(pLayer == null)){            
                if (pLayer is IFeatureLayer)
                {
                    pFLayer = pLayer as IFeatureLayer2;
                    if (FT == esriGeometryType.esriGeometryAny)
                        col.Add(pLayer.Name);
                    else if (pFLayer.ShapeType == FT)
                        col.Add(pLayer.Name);
                }
                pLayer = pLayers.Next();
            }
            return col;
        }
        /// <summary>Retorna un ArrayList de los valores contenidos en una columna</summary>
        /// <param name="layer">Layer o grupo a buscar</param>
        /// <param name="campo">Campo de origen de valores.</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <returns> Devuelve un ArrayList.</returns>
        public static ArrayList listadeValores(IMxDocument pMxDoc, string layer, string campo)
        {
            ArrayList colVals = new ArrayList();
            IFeatureClass fc = default(IFeatureClass);
            fc = returnFeatureLayerByName(pMxDoc, layer).FeatureClass;
            colVals = returnFieldData(fc, campo);
            return colVals;
        }
        /// <summary>Realiza una seleccion según una condición</summary>
        /// <param name="condicion">Condición para realizar la selección</param>
        /// <param name="Layer">Layer de origen de valores.</param>
        /// <param name="b">Indica si la seleccion se agrega a la existente</param>
        /// <param name="activeView"> Visualizacion activa.</param>
        /// <returns>Retorna un IFeatureSelection.</returns>
        public static IFeatureSelection seleccionByAttributeQuery(string condicion, ILayer Layer, bool b=false,IActiveView activeView=null)
        {
            IQueryFilter queryFilter = new QueryFilter();
            IFeatureLayer pfLayer= (IFeatureLayer)Layer;
            try {
                queryFilter.WhereClause = condicion;
                 IFeatureSelection featureSelection = pfLayer as IFeatureSelection;
              /*  if (activeView != null)
                    activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);*/
                if (b)
                    featureSelection.SelectFeatures(queryFilter, ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultAdd, false);
                else
                    featureSelection.SelectFeatures(queryFilter, ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultNew, false);
                if (activeView != null)
                    activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                return Layer as IFeatureSelection;
            }
            catch (System.Exception ex){
                throw ex;
            }
        }
        /// <summary>Remueve una capa de la tabla de contenidos</summary>
        /// <param name="capa">Nombre de la capa</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        public static void limpiarZonasTmp(IMxDocument pMxDoc, string capa){
            try {
                IMap pMap = pMxDoc.FocusMap;
                pMap.DeleteLayer(returnLayerByName(pMxDoc, capa));
            }catch (System.Exception ex) {
                throw new ArgumentException("selectClip \n Error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
        /// <summary>Remueve la selección de todos los elementos</summary>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        public static void limpiarLayerTmp(IMxDocument pMxDoc)
        {
            IMap pMap = pMxDoc.FocusMap;
            pMap.ClearSelection();
        }
        /// <summary>Realza la conversion de polygono a polilineas</summary>        
        /// <param name="pElementofPolygon">Nombre de la capa</param>
        /// <returns>Retorna un IPolyline.</returns>
        public static IPolyline createPolylineFromPolygon( IGeometry pElementofPolygon){
            try{
                IPolyline pPolyline = default(IPolyline);
                IPolygon tmp = (IPolygon)pElementofPolygon;
                pPolyline = (IPolyline)polygonToPolyline(tmp);
                return pPolyline;
            } catch (System.Exception ex) {
                throw ex;
            }
        }
        /// <summary>Realza la conversion de polygono a polilineas</summary>        
        /// <param name="pElementofPolygon">Nombre de la capa</param>
        /// <returns>Retorna un IPolyline.</returns>
        public static IPolygon createPolygonFromPolygon(IGeometry pElementofPolygon)
        {
            try
            {
                IPolygon pPolyline = default(IPolygon);
                IPolygon tmp = (IPolygon)pElementofPolygon;
                pPolyline = (IPolygon)/*polygonToPolyline*/(tmp);
                return pPolyline;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        private static IGeometryCollection polygonToPolyline(IPolygon pPolygon) {
            try {
                IGeometryCollection polygonToPolyline = new PolylineClass();
                IClone pClone;
                IGeometryCollection pGeoms_Polygon;
                ISegmentCollection pSegs_Path;
                pClone = (IClone)pPolygon;
                pGeoms_Polygon = (IGeometryCollection)pClone.Clone();
                for (int i = 0;i < pGeoms_Polygon.GeometryCount; i++) {
                    pSegs_Path = new PathClass() as ISegmentCollection;
                    pSegs_Path.AddSegmentCollection((ISegmentCollection)pGeoms_Polygon.get_Geometry(i));
                    polygonToPolyline.AddGeometry((IGeometry)pSegs_Path);
                }
                return polygonToPolyline;} catch (System.Exception ex) {
                throw ex;
            }
        }
        /// <summary>Inicia edicion en un layer especifico</summary>
        /// <param name="m_mapControl">IMxDocument .FocusMap.</param>
        /// <param name="tipo">Opcion para la edicion</param>
        /// <param name="cLayer">Layer a editar</param>
        public static void startEditing(IMap m_mapControl, String tipo, ILayer cLayer){        
            try
            {
            IEngineEditor m_engineEditor = new EngineEditorClass();
            switch (tipo) {
                case "Terminar Guardando":
                    m_engineEditor.StopOperation("");
                    m_engineEditor.StopEditing(true);
                    break;
                case "Terminar Sin Guardar":
                    m_engineEditor.StopEditing(false);
                    break;
                case "Editar":
                    if (m_engineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing) m_engineEditor.StopEditing(false);
                    if (cLayer is IFeatureLayer) {
                        IFeatureLayer featureLayer = (IFeatureLayer)cLayer;
                        ESRI.ArcGIS.Geodatabase.IDataset dataset = featureLayer.FeatureClass as ESRI.ArcGIS.Geodatabase.IDataset;
                        ESRI.ArcGIS.Geodatabase.IWorkspace workspace = dataset.Workspace;
                        m_engineEditor.StartEditing(workspace, m_mapControl);
                        ((IEngineEditLayers)m_engineEditor).SetTargetLayer(featureLayer, 0);
                        m_engineEditor.StartOperation();
                    }
                    break;
                case "Guardar":
                    if (m_engineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing) m_engineEditor.StopEditing(true);
                    break;
            }
            }
            catch (System.Exception ex)
            {
                throw new ArgumentException("startEditing \n Error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
        /// <summary>Realza un Zoom -In al elemento seleccionado dentro de la capa</summary>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <param name="capa">Nombre de la capa</param>
        public static void zoomToSeleccion(IMxDocument pMxDoc, string capa)
        {
            ILayer pLayer = default(ILayer);
            IFeatureSelection pfSelection = default(IFeatureSelection);
            ISelectionSet pSelSet = default(ISelectionSet);
            IEnumGeometry pEnumGeom = default(IEnumGeometry);
            IEnumGeometryBind pEnumGeomBind;
            IGeometryFactory pGeomFactory;
            IGeometry pGeom = default(IGeometry);
            try {
                pLayer = returnLayerByName(pMxDoc, capa);
                pfSelection = (IFeatureSelection)pLayer;
                pSelSet = pfSelection.SelectionSet;
                pEnumGeom = new EnumFeatureGeometry();
                pEnumGeomBind = (IEnumGeometryBind)pEnumGeom;
                pEnumGeomBind.BindGeometrySource(null, pSelSet);
                pGeomFactory = (IGeometryFactory)new GeometryEnvironment();
                pGeom = pGeomFactory.CreateGeometryFromEnumerator(pEnumGeom);
                pMxDoc.ActiveView.Extent = pGeom.Envelope;
            } catch (System.Exception ex) {
                throw ex;
            }
        }
        /*
         public void iFeatureCursor_UpdateFeaure(IFeatureClass featureClass, string condicion,string campo, string upDate)
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = campo+condicion;
            IFeatureCursor updateCursor = featureClass.Update(queryFilter, false);
            int fieldIndex = featureClass.FindField(campo);
            IFeature feature = null;
            while ((feature = updateCursor.NextFeature()) != null){
                feature.set_Value(fieldIndex, upDate); //"C" = domain value for Commercial
                updateCursor.UpdateFeature(feature);
            }
            //Marshal.ReleaseComObject(updateCursor);
        }
         */
        private static void insertDatoS(IFeatureClass pFeatureClass, ref IFeature pFeature, string[,] elObjeto)
        {
            try
            {
                int n = elObjeto.GetLength(0);
                for (int i = 0; i < n; i++)
                {//Se recorre el arrays y actualiza los valores
                    int contractorFieldIndex = pFeatureClass.FindField(elObjeto[i, 0].ToString());
                    if (contractorFieldIndex >= 0) {
                        string sw = elObjeto[i, 1].ToString();
                        switch (sw) {
                            case "double":
                            case "Double":
                                double dvalor = double.Parse(elObjeto[i, 2].ToString());
                                pFeature.set_Value(contractorFieldIndex, dvalor);
                                break;
                            case "Int":
                            case "int":
                            case "Single":
                            case "Int16":
                            case "Int32":
                                int pvalor = Int32.Parse(elObjeto[i, 2].ToString());
                                pFeature.set_Value(contractorFieldIndex, pvalor);
                                break;
                            case "Str":
                            case "string":
                            case "String":
                                pFeature.set_Value(contractorFieldIndex, elObjeto[i, 2].ToString());
                                break;
                            case "Date":
                                if (elObjeto[i, 2].ToString() == "hoy")
                                    pFeature.set_Value(contractorFieldIndex, DateTime.Today);
                                break;
                    }
                }
                }
            }
            catch (System.Exception ex) { throw ex; }
        }
        /// <summary>Realza un insert nuevos datos en u l</summary>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <param name="capa">Nombre de la capa</param>
        /// <param name="queryFilter">IQueryFilter que selcciono en la capa</param>
        /// <param name="myFeature"> El IFeature del elemento a seleccionado</param> 
        /// <param name="elObjeto">Arreglo de Objeto con los datos (nombre del campo,tipo de valor,valor).</param>
        public static void insertDatosLayer(IMxDocument pMxDoc, string capa, IQueryFilter queryFilter,  IFeature myFeature, string[,] elObjeto)
        {
            try
            {
                IActiveView activeView = pMxDoc.ActivatedView;
                ILayer olayer = returnLayerByName(pMxDoc, capa, activeView);
                if (myFeature == null) {
                    //MessageBox.Show("Feature vacio");
                    throw new ArgumentException("Feature vacio");
                }
                startEditing(activeView.FocusMap, "Editar", olayer);
                IFeatureClass featureClass = returnFeatureClassByName(pMxDoc, capa);
                IFeatureCursor pFeatCur = featureClass.Update(queryFilter, false);
                IFeature pFeat = pFeatCur.NextFeature();
                if (elObjeto != null) {
                    insertDatoS(featureClass, ref pFeat, elObjeto);
                    pFeatCur.UpdateFeature(pFeat);
                    pFeat = pFeatCur.NextFeature();
                }
                pFeatCur = null;
                // Stop the edit operation.
                startEditing(activeView.FocusMap, "Terminar Guardando", olayer);

            } catch (System.Exception ex) { throw ex; }
        }
        /// <summary>Realza un Zoom -In al elemento seleccionado dentro de la capa</summary>
        /// <param name="outputFeatureClass">IFeatureClass destino</param>
        /// <param name="inFeatureClass">IFeatureClass origen</param>
        /// <param name="select">clausula para selccionar de la capa origen</param>
        /// <param name="campos">campos para selccionar de la capa origen</param>
        public static void loadObjects( IFeatureClass outputFeatureClass, IFeatureClass inFeatureClass, string select,string campos)
        {
            try {
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.SubFields = campos;
                queryFilter.WhereClause = select;
                IFields allFields = outputFeatureClass.Fields;
                IFields outFields = new FieldsClass();
                IFieldsEdit outFieldsEdit = outFields as IFieldsEdit;
                String[] subFields = (queryFilter.SubFields).Split(',');
                for (int j = 0; j < subFields.Length; j++){
                    int fieldID = allFields.FindField(subFields[j]);
                    if (fieldID == -1)
                    {
                        throw new ArgumentException("EL campo " + subFields[j]+" no fue encontrado");
                       // return;
                    }
                    outFieldsEdit.AddField(allFields.get_Field(fieldID));
                }

                IObjectLoader objectLoader = new ObjectLoader();
                IEnumInvalidObject invalidObjectEnum=null;
                objectLoader.LoadObjects(
                    null,
                    (ITable)inFeatureClass,
                    queryFilter,
                    (ITable)outputFeatureClass,
                    outFields,
                    false,
                    0,
                    false,
                    false,
                    10,
                    out invalidObjectEnum
                );
                IInvalidObjectInfo invalidObject = invalidObjectEnum.Next();
                if (invalidObject != null)
                    throw new ArgumentException("Algunos o todos los features no se cargaron");
            }
            catch (System.Exception ex) { throw ex; }
        }
        /// <summary> Copia y pega un elemento en un capa destino agregando los campos</summary>
        /// <param name="myfeatureOrigen">feature (poligono) selecciono </param>
        /// <param name="capaDestino">Layer a editar y pegar el feature</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <param name="campos">Objeto con los datos (nombre del campo,tipo de valor,valor).</param>
        public static bool copiaPoligono(IFeature myfeatureOrigen, string capaDestino, IMxDocument pMxDoc,  string[,] campos)
        {
            try
            {
                IActiveView activeView = pMxDoc.ActivatedView;
                ILayer layer= returnLayerByName(pMxDoc, capaDestino, activeView);
                startEditing(activeView.FocusMap, "Editar", layer);
                //elimina los colindantes que exitian
                if (myfeatureOrigen == null)
                    return false;
                ESRI.ArcGIS.Geometry.IPolygon linepart = createPolygonFromPolygon(myfeatureOrigen.Shape);
                if (linepart == null)
                    return false;
                if (linepart.IsEmpty)
                    return false;
               /* ISegmentCollection pSegmentCollection;
                pSegmentCollection = (ISegmentCollection)linepart;
                */
               // linepart = null;
                int elfid;
                elfid = createFeature(capaDestino, linepart, pMxDoc, layer, campos);               
                startEditing(activeView.FocusMap, "Terminar Guardando", layer);
                return true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("copiaPoligono \nError: " + ex.Message + "\n" + ex.StackTrace);
               // return false;
            }
        }
    }
}
