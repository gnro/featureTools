using System;
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


namespace featureTools
{
    /// <summary>Proporciona herramientas básicas para trabajar con Capas</summary>
    /// <remarks>Realiza operaciones de consulta y cambio de propiedades de las capas en ArcMap</remarks>
    public class feature
    {
        /// <summary> Retorna un IFeatureCursor en base a un nombre de layer</summary>
        /// <param name="layerName"> Nombre de la capa del elemento a seleccionar.</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        /// <param name="envelope"> IEnvelope.</param>
        /// <param name="eltiporelacion"> El tipo de relacion.</param>
        public static IFeature selectedfeature(string layerName, IActiveView ActiveView, IEnvelope envelope, esriSpatialRelEnum eltiporelacion)
        {
            IFeatureCursor featureCursor = selectedFeature(layerName, ActiveView, envelope, eltiporelacion);
            IFeature feature = featureCursor.NextFeature();
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            return feature;
        }
        /// <summary> Retorna un IFeatureCursor </summary>
        /// <param name="layerName"> Nombre de la capa del elemento a seleccionar.</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        /// <param name="envelope"> IEnvelope.</param>
        /// <param name="eltiporelacion"> El tipo de relacion.</param>
        public static IFeatureCursor selectedfeatures(string layerName, IActiveView ActiveView, IEnvelope envelope, esriSpatialRelEnum eltiporelacion)
        {
            return selectedFeature(layerName, ActiveView, envelope, eltiporelacion);
        }
        /// <summary> Retorna un IFeatureCursor </summary>
        /// <param name="layerName"> Nombre de la capa del elemento a seleccionar.</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        /// <param name="envelope"> IEnvelope.</param>
        /// <param name="eltiporelacion"> El tipo de relacion.</param>
        public static IFeatureCursor selectedFeature(string layerName, IActiveView ActiveView, IEnvelope envelope, esriSpatialRelEnum eltiporelacion){
            try{
                IMap map = ActiveView.FocusMap;
                ILayer layer = returnLayerByName(null, layerName, ActiveView, ActiveView.FocusMap);
                IFeatureLayer pFeatureLayer = (IFeatureLayer)layer;
                IFeatureClass featureClass = pFeatureLayer.FeatureClass;
                System.String shapeFieldName = featureClass.ShapeFieldName;
                // Create a new spatial filter and use the new envelope as the geometry
                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.Geometry = envelope;
                spatialFilter.SpatialRel = (esriSpatialRelEnum)eltiporelacion;
                //spatialFilter.OutputSpatialReference(shapeFieldName) = map.SpatialReference;
                spatialFilter.set_OutputSpatialReference(shapeFieldName, map.SpatialReference);
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
        /// <param name="lacapa"> Nombre de la capa del elemento a seleccionar</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        public static IEnvelope selectByPoint(int x, int y, string lacapa, IActiveView ActiveView)
        {
            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = ActiveView.ScreenDisplay;
            ESRI.ArcGIS.Display.IDisplayTransformation displayTransformation = screenDisplay.DisplayTransformation;
            ESRI.ArcGIS.Geometry.Point pnt = new ESRI.ArcGIS.Geometry.Point();
            pnt = (Point)displayTransformation.ToMapPoint(x, y);
            IEnvelope envelope = pnt.Envelope;
            return envelope;
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
        public static double[] intToCentroide(int x, int y, string lacapa, IActiveView ActiveView)
        {
            IFeature myfeature = selectPoint(x, y, lacapa, ActiveView);
            return ifeatureCentroide(myfeature);
        }
        /// <summary> Retorna las coordenadas de un punto </summary>
        /// <param name="myfeature"> El IFeature del elemento a seleccionar</param>
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
        public static int[] pointToMap(IPoint mapPoint, IActiveView ActiveView)
        {
            if (mapPoint == null || mapPoint.IsEmpty || ActiveView == null)
            {
                return null;
            }
            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = ActiveView.ScreenDisplay;
            ESRI.ArcGIS.Display.IDisplayTransformation displayTransformation =
                screenDisplay.DisplayTransformation;
            int[] c = { 0, 0 };
            displayTransformation.FromMapPoint(mapPoint, out c[0], out c[1]);
            return c;
        }
        /// <summary> Permiete seleccionar un elemento de tipo punto </summary>
        /// <param name="x"> coordenada x.</param>
        /// <param name="y"> coordenada y.</param>
        /// <param name="lacapa"> Nombre de la capa del elemento a seleccionar</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        public static IFeature selectPoint(int x, int y, string lacapa, IActiveView ActiveView)
        {
            IFeature myfeature;
            IEnvelope envelope = selectByPoint(x, y, ActiveView);
            envelope.Expand(500, 500, false);
            myfeature = selectedfeature(lacapa, ActiveView, envelope, esriSpatialRelEnum.esriSpatialRelContains);
            return myfeature;
        }
        /// <summary> Permiete seleccionar un elemento de tipo poligono </summary>
        /// <param name="x"> coordenada x.</param>
        /// <param name="y"> coordenada y.</param>
        /// <param name="lacapa"> Nombre de la capa del elemento a seleccionar</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        public static IFeature selectPolygon(int x, int y, string lacapa, IActiveView ActiveView)
        {
            IFeature myfeature;
            IEnvelope envelope = selectByPoint(x, y,  ActiveView);
            envelope.Expand(0.5, 0.05, false);
            myfeature = selectedfeature(lacapa, ActiveView, envelope, esriSpatialRelEnum.esriSpatialRelWithin);
            return myfeature;
        }
        /// <summary> Permiete seleccionar un elemento de tipo linea </summary>
        /// <param name="x"> coordenada x.</param>
        /// <param name="y"> coordenada y.</param>
        /// <param name="lacapa"> Nombre de la capa del elemento a seleccionar</param>
        /// <param name="ActiveView"> Visualizacion activa.</param>
        public static IFeature selectLine(int x, int y, string lacapa, IActiveView ActiveView)
        {
            IFeature myfeature;
            IEnvelope envelope = selectByPoint(x, y,  ActiveView);
            envelope.Expand(1, 1, false);
            myfeature = selectedfeature(lacapa, ActiveView, envelope, esriSpatialRelEnum.esriSpatialRelIntersects);
            return myfeature;
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
        /// <summary>Retorna una layer en base al nombre</summary>
        /// <param name="layer">Nombre del layer a buscar.</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        /// <returns>Devuelve un ILayer.</returns>
        public static ILayer returnLayerByName(IMxDocument pMxDoc, string layer, IActiveView activeView = null, IMap pMap=null)
        {
            if (pMap ==null)
                pMap = pMxDoc.FocusMap;
            if (activeView!=null)
                pMap = activeView.FocusMap;
            IEnumLayer pLayers = pMap.Layers;
            ILayer pLayer;
            pLayers = pMap.Layers;
            pLayer = pLayers.Next();
            while (!(pLayer == null)) {
                if (pLayer.Name == layer)
                    return pLayer;
                pLayer = pLayers.Next();
            }
            return null;
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
            pLayers = pMap.Layers;
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
            return col;
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
            pLayers = pMap.Layers;
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
            pLayers = pMap.Layers;
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
        /// <summary>Realiza una seleccion según una condición</summary>
        /// <param name="condicion">Condición para realizar la selección</param>
        /// <param name="Layer">Layer de origen de valores.</param>
        /// <param name="b">Indica si la seleccion se agrega a la existente</param>
        /// <returns> Devuelve un entero.</returns>
        public static IFeatureSelection seleccionByAttributeQuery(string condicion, ILayer Layer, bool b=false)
        {
            IQueryFilter pqueryfilter = default(IQueryFilter);
            IFeatureLayer pfLayer;
            try {
                pqueryfilter = new QueryFilter();
                pfLayer = (IFeatureLayer)Layer;
                pqueryfilter.WhereClause = condicion;
                ESRI.ArcGIS.Carto.IFeatureSelection featureSelection = pfLayer as ESRI.ArcGIS.Carto.IFeatureSelection;
                if (b)
                    featureSelection.SelectFeatures(pqueryfilter, ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultAdd, false);
                else
                    featureSelection.SelectFeatures(pqueryfilter, ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultNew, false);
                return Layer as IFeatureSelection;
            }
            catch (System.Exception ex){
                throw ex;
            }
        }
        /// <summary>Remueve una capa de la tabla de contenidos</summary>
        /// <param name="capa">Nombre de la capa</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        public static void limpiarZonasTmp(IMxDocument pMxDoc, string capa)
        {
            IMap pMap = pMxDoc.FocusMap;
            pMap.DeleteLayer(returnLayerByName(pMxDoc, capa));
        }
        /// <summary>Remueve la selección de todos los elementos</summary>
        /// <param name="pMxDoc">ArcMap.Document.</param>
        public static void limpiarLayerTmp(IMxDocument pMxDoc)
        {
            IMap pMap = pMxDoc.FocusMap;
            pMap.ClearSelection();
        }
        /// <summary>Realza un Zoom -In al elemento seleccionado dentro de la capa</summary>
        /// <param name="capa">Nombre de la capa</param>
        /// <param name="pMxDoc">ArcMap.Document.</param>
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

        /// <summary>Inicia edicion en un layer especifico</summary>
        /// <param name="m_mapControl">IMxDocument .FocusMap.</param>
        /// <param name="tipo">Opcion para la edicion</param>
        /// <param name="cLayer">Layer a editar</param>
        public static void startEditing(IMap m_mapControl, String tipo, ILayer cLayer)
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

    }
    //private class objetos
    //{
    //    public string clave { get; set; }
    //    public string valor { get; set; }
    //}
}