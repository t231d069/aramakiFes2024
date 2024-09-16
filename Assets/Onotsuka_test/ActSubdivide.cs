using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// 切断対象オブジェクトの参照

// Mesh.positions Mesh.normal Mesh.triangle Mesh.uv を取得

// 参照したオブジェクトのメッシュのすべての頂点に対して，無限平面のどちらにあるかを判定する

// 左・右判定された頂点を保持する 

// 左右のばらけているメッシュに対して，新たな頂点を生成する

// すべての頂点に対してポリゴンを形成する

// 切断面の定義，新しいマテリアルの適用
public class ActSubdivide : MonoBehaviour {

    [SerializeField] private GameObject newGameObjectPrefab;

    private int[] targetTriangles;
    private Vector3[] targetVertices;
    private Vector3[] targetNormals;
    private Vector2[] targetUVs;

    private void Start() {
        Plane cutter = new Plane(transform.right, transform.position);
        Subdivide(cutter);
        Destroy(this.gameObject);
    }

    public void Subdivide(Plane cutter) {
        // 切断対象のオブジェクトのメッシュ情報
        Mesh targetMesh = this.GetComponent<MeshFilter>().mesh;
        targetTriangles = targetMesh.triangles;
        targetVertices  = targetMesh.vertices;
        targetNormals   = targetMesh.normals;
        targetUVs       = targetMesh.uv;

        // 切断対象のオブジェクトの情報操作用
        int             targetVerticesLength  = targetVertices.Length;
        List<int>       irrelevantTriangles   = new List<int>();
        List<Vector3>   targetVerticesList    = new List<Vector3>(targetVertices);
        List<Vector3>   newVerticesList       = new List<Vector3>();
        List<int[]>     vertexPairList        = new List<int[]>();
        List<List<int>> joinedVertexGroupList = new List<List<int>>();
        Vector2[]       new2DVerticesArray;
        string[]        vertexType;

        // 切断面左側のオブジェクトのメッシュ情報
        List<int>     leftTriangles = new List<int>();
        List<Vector3> leftVertices  = new List<Vector3>();
        List<Vector3> leftNormals   = new List<Vector3>();
        List<Vector2> leftUVs       = new List<Vector2>();
        // 切断面右側のオブジェクトのメッシュ情報
        List<int>     rightTriangles = new List<int>();
        List<Vector3> rightVertices  = new List<Vector3>();
        List<Vector3> rightNormals   = new List<Vector3>();
        List<Vector2> rightUVs       = new List<Vector2>();
        // 切断対象のオブジェクトの各ポリゴンの左右判定用
        bool vertexTruthValue1, vertexTruthValue2, vertexTruthValue3;

        /* **************************** */
        /* 断面の左右のメッシュを生成する */
        /* **************************** */

        for (int i = 0; i < targetTriangles.Length; i += 3) {
            vertexTruthValue1 = cutter.GetSide(targetVertices[targetTriangles[i]]);
            vertexTruthValue2 = cutter.GetSide(targetVertices[targetTriangles[i + 1]]);
            vertexTruthValue3 = cutter.GetSide(targetVertices[targetTriangles[i + 2]]);
            //対象の三角形ポリゴンの頂点すべてが右側にある場合
            if (vertexTruthValue1 && vertexTruthValue2 && vertexTruthValue3) {
                AddToRightSide(
                    i, 
                    targetTriangles, 
                    targetUVs, 
                    rightTriangles, 
                    rightUVs
                );
            }
            // 対象の三角形ポリゴンの頂点すべてが左側にある場合
            else if (!vertexTruthValue1 && !vertexTruthValue2 && !vertexTruthValue3) {
                AddToLeftSide(
                    i, 
                    targetTriangles, 
                    targetUVs, 
                    leftTriangles, 
                    leftUVs
                );
            }
            // 対象の三角形ポリゴンの頂点が左右に分かれている場合
            else {
                ProcessMixedTriangle(
                    i, 
                    cutter,
                    vertexTruthValue1,
                    vertexTruthValue2,
                    vertexTruthValue3,
                    targetTriangles,
                    targetVertices,
                    targetUVs,
                    targetVerticesList,
                    newVerticesList,
                    vertexPairList,
                    rightUVs,
                    leftUVs,
                    rightTriangles,
                    leftTriangles
                );
            }
        }
        /* ************************ */
        /* 断面上のメッシュを生成する */
        /* ************************ */
        
        // 新頂点の二次元座標変換する
        new2DVerticesArray = new Vector2[newVerticesList.Count];
        new2DVerticesArray = GeometryUtils.ConvertCoordinates3DTo2D(cutter, newVerticesList);
        // ひとつなぎの辺で形成されるすべての図形をリストアップする
        joinedVertexGroupList = GeometryUtils.GroupingForDetermineGeometry(vertexPairList, joinedVertexGroupList);
        // 最も外郭となる処理図形 (内包図形の有無に関わらない) ごとにグループ化する
        List<List<int>> nonConvexGeometryList = GeometryUtils.GroupingForSegmentNonMonotoneGeometry(new2DVerticesArray, joinedVertexGroupList);
        // 新頂点を種類ごとに分類する
        vertexType = new string[newVerticesList.Count];
        vertexType = GeometryUtils.ClusteringVertexType(new2DVerticesArray, joinedVertexGroupList);
        // 処理図形グループをもとに，処理図形ごとの頂点リストと辺リストを生成する
        (
            int [][] nonConvexGeometryVerticesJagAry,
            int [][][] nonConvexGeometryEdgesJagAry
        ) = GeometryUtils.EdgeForMakeMonotone(
            nonConvexGeometryList, 
            joinedVertexGroupList
        );
        // 処理図形グループの頂点リストを y 座標降順でソートする
        )

    }
    private void AddToRightSide(
        int i, 
        int[]         targetTriangles, 
        Vector2[]     targetUVs, 
        List<int>     rightTriangles, 
        List<Vector2> rightUVs
    ) {
        for (int k = 0; k < 3; k++) {
            rightUVs.Add(targetUVs[targetTriangles[i + k]]);
            rightTriangles.Add(targetTriangles[i + k]);
        }
    }
    private void AddToLeftSide(
        int i, 
        int[]         targetTriangles, 
        Vector2[]     targetUVs, 
        List<int>     leftTriangles, 
        List<Vector2> leftUVs
    ) {
        for (int k = 0; k < 3; k++) {
            leftUVs.Add(targetUVs[targetTriangles[i + k]]);
            leftTriangles.Add(targetTriangles[i + k]);
        }
    }
    private void ProcessMixedTriangle(
        int i, 
        Cutter cutter, 
        bool vertexTruthValue1, 
        bool vertexTruthValue2, 
        bool vertexTruthValue3, 
        int[]         targetTriangles, 
        Vector3[]     targetVertices, 
        Vector2[]     targetUVs, 
        List<Vector3> targetVerticesList, 
        List<Vector3> newVerticesList, 
        List<int[]>   vertexPairList, 
        List<Vector2> rightUVs, 
        List<Vector2> leftUVs,
        List<int>     rightTriangles,
        List<int>     leftTriangles
    ) {
        ( // ポリゴンの頂点情報を扱いやすいように整理する
            bool rtlf, 
            int vertexIndex1, Vector3 lonelyVertex, 
            int vertexIndex2, Vector3 startPairVertex, 
            int vertexIndex3, Vector3 lastPairVertex
        ) = SegmentedPolygonsUtils.SortIndex(
            targetTriangles[i], vertexTruthValue1, targetVertices[targetTriangles[i]],
            targetTriangles[i + 1], vertexTruthValue2, targetVertices[targetTriangles[i + 1]],
            targetTriangles[i + 2], vertexTruthValue3, targetVertices[targetTriangles[i + 2]]
        );
        ( // 新しい頂点を生成する
            Vector3 newStartPairVertex, 
            Vector3 newLastPairVertex, 
            float ratio_LonelyAsStart, 
            float ratio_LonelyAsLast
        ) = SegmentedPolygonsUtils.GenerateNewVertex(
            cutter, rtlf, lonelyVertex, startPairVertex, lastPairVertex
        );
        ( // 新しいUV座標を生成する
            Vector2 newUV1, 
            Vector2 newUV2
        ) = SegmentedPolygonsUtils.GenerateNewUV (
            ratio_LonelyAsStart, 
            ratio_LonelyAsLast,
            targetUVs[vertexIndex1], 
            targetUVs[vertexIndex2], 
            targetUVs[vertexIndex3]
        );
        ( // 重複頂点の処理を行う (辺の始点)
            bool deltrueSV, 
            int newVertexIndexSV
        ) = SegmentedPolygonsUtils.InsertAndDeleteVertices (
            targetVerticesLength, 
            newStartPairVertex, 
            newVerticesList
        );
        if (deltrueSV == false) {
            newVerticesList.Add(newStartPairVertex);
            targetVerticesList.Add(newStartPairVertex);
        }
        ( // 重複頂点の処理を行う (辺の終点)
            bool deltrueLV, 
            int newVertexIndexLV
        ) = SegmentedPolygonsUtils.InsertAndDeleteVertices (
            targetVerticesLength, 
            newLastPairVertex, 
            newVerticesList
        );
        if (deltrueLV == false) {
            newVerticesList.Add(newLastPairVertex);
            targetVerticesList.Add(newLastPairVertex);
        }
        // のちに頂点インデックスをもとに，こいつはこいつで頂点グルーピングするので保存しておく
        int [] newVertexSet =  new int[] {newVertexIndexSV - targetVerticesLength, newVertexIndexLV - targetVerticesLength};
        vertexPairList.Add(newVertexSet);

        /* ********************************* */
        /* 孤独な頂点が無限平面の右側にある場合 */
        /* ********************************* */
        if (rtlf) {
            // 切断ポリゴン右側を生成する処理
            rightUVs.Add(targetUVs[vertexIndex1]);
            rightUVs.Add(newUV1);
            rightUVs.Add(newUV2);
            rightTriangles.Add(vertexIndex1);
            rightTriangles.Add(newVertexIndexSV);
            rightTriangles.Add(newVertexIndexLV);
            // 切断ポリゴン左側一つ目を生成する処理
            leftUVs.Add(newUV1);
            leftUVs.Add(targetUVs[vertexIndex2]);
            leftUVs.Add(targetUVs[vertexIndex3]);
            leftTriangles.Add(newVertexIndexSV);
            leftTriangles.Add(vertexIndex2);
            leftTriangles.Add(vertexIndex3);
            // 切断ポリゴン左側二つ目を生成する処理
            leftUVs.Add(targetUVs[vertexIndex3]);
            leftUVs.Add(newUV2);
            leftUVs.Add(newUV1);
            leftTriangles.Add(vertexIndex3);
            leftTriangles.Add(newVertexIndexLV);
            leftTriangles.Add(newVertexIndexSV);
        }
        /* ********************************* */
        /* 孤独な頂点が無限平面の左側にある場合 */
        /* ********************************* */
        else {
            // 切断ポリゴン左側を生成する処理
            leftUVs.Add(targetUVs[vertexIndex1]);
            leftUVs.Add(newUV1);
            leftUVs.Add(newUV2);
            leftTriangles.Add(vertexIndex1);
            leftTriangles.Add(newVertexIndexLV);
            leftTriangles.Add(newVertexIndexSV);
            // 切断ポリゴン右側一つ目を生成する処理
            rightUVs.Add(newUV1);
            rightUVs.Add(targetUVs[vertexIndex2]);
            rightUVs.Add(targetUVs[vertexIndex3]);
            rightTriangles.Add(newVertexIndexLV);
            rightTriangles.Add(vertexIndex2);
            rightTriangles.Add(vertexIndex3);
            // 切断ポリゴン右側二つ目を生成する処理
            rightUVs.Add(targetUVs[vertexIndex3]);
            rightUVs.Add(newUV2);
            rightUVs.Add(newUV1);
            rightTriangles.Add(vertexIndex3);
            rightTriangles.Add(newVertexIndexSV);
            rightTriangles.Add(newVertexIndexLV);
        }
    }
}

// 分断ポリゴンに対する処理系
public static class SegmentedPolygonsUtils {
    // ポリゴンの頂点番号を，孤独な頂点を先頭に，表裏情報をもつ順番に並び替える
    public static (
        bool rtlf, 
        int newIndex1, Vector3 lonelyVertex, 
        int newIndex2, Vector3 startPairVertex, 
        int newIndex3, Vector3 lastPairVertex
    ) SortIndex(
        int index1, bool vertexTruthValue1, Vector3 vertex1, 
        int index2, bool vertexTruthValue2, Vector3 vertex2, 
        int index3, bool vertexTruthValue3, Vector3 vertex3
    ) {
        // 孤独な頂点が無限平面の右側にある場合
        if (vertexTruthValue1 && !vertexTruthValue2 && !vertexTruthValue3) {
            bool rtlf = true;
            return (rtlf, index1, vertex1, index2, vertex2, index3, vertex3);
        }
        else if (!vertexTruthValue1 && vertexTruthValue2 && !vertexTruthValue3) {
            bool rtlf = true;
            return (rtlf, index2, vertex2, index3, vertex3, index1, vertex1);
        }
        else if (!vertexTruthValue1 && !vertexTruthValue2 && vertexTruthValue3) {
            bool rtlf = true;
            return (rtlf, index3, vertex3, index1, vertex1, index2, vertex2);
        }
        // 孤独な頂点が無限平面の左側にある頂点
        else if (vertexTruthValue1 && vertexTruthValue2 && !vertexTruthValue3) {
            bool rtlf = false;
            return (rtlf, index3, vertex3, index1, vertex1, index2, vertex2);
        }
        else if (vertexTruthValue1 && !vertexTruthValue2 && vertexTruthValue3) {
            bool rtlf = false;
            return (rtlf, index2, vertex2, index3, vertex3, index1, vertex1);
        }
        else { // if (!vertexTruthValue1 && vertexTruthValue2 && vertexTruthValue3)
            bool rtlf = false;
            return (rtlf, index1, vertex1, index2, vertex2, index3, vertex3);
        }
    }

    // ポリゴンの切断辺の両端の頂点を，切断ポリゴンの法線・切断平面の法線とフレミングの左手の方向になるように生成する
    public static (
        Vector3 newStartPairVertex, 
        Vector3 newLastPairVertex,
        float ratio_LonelyStart,
        float ratio_LonelyLast
    ) GenerateNewVertex(
        Plane plane, 
        bool rtlf, 
        Vector3 lonelyVertex, 
        Vector3 startPairVertex, 
        Vector3 lastPairVertex
    ) {
        Ray ray1 = new Ray(lonelyVertex, startPairVertex - lonelyVertex);
        Ray ray2 = new Ray(lonelyVertex, lastPairVertex - lonelyVertex);
        float distance1 = 0.0f;
        plane.Raycast(ray1, out distance1);
        Vector3 newStartPairVertex = ray1.GetPoint(distance1);
        float distance2 = 0.0f;
        plane.Raycast(ray2, out distance2);
        Vector3 newLastPairVertex = ray2.GetPoint(distance2);

        float ratio_LonelyStart = distance1 / Vector3.Distance(lonelyVertex, startPairVertex);
        float ratio_LonelyLast = distance2 / Vector3.Distance(lonelyVertex, lastPairVertex);

        if (rtlf) {
            return (newStartPairVertex, newLastPairVertex, ratio_LonelyStart, ratio_LonelyLast);
        }
        else {
            return (newLastPairVertex, newStartPairVertex, ratio_LonelyStart, ratio_LonelyLast);
        }
    }

    // 新頂点のUV座標を生成する
    public static (
        Vector2 newUV1, 
        Vector2 newUV2
    ) GenerateNewUV(
        float ratio_LonelyAsStart, 
        float ratio_LonelyAsLast, 
        Vector2 uv1, Vector2 uv2, Vector2 uv3
    ) {
        Vector2 newUV1 = new Vector2(
            uv1.x + (uv2.x - uv1.x) * ratio_LonelyAsStart,
            uv1.y + (uv2.y - uv1.y) * ratio_LonelyAsStart
        );
        Vector2 newUV2 = new Vector2(
            uv1.x + (uv3.x - uv1.x) * ratio_LonelyAsLast,
            uv1.y + (uv3.y - uv1.y) * ratio_LonelyAsLast
        );
        return (newUV1, newUV2);
    }

    // 重複する頂点を削除する
    public static (
        bool deltrue, 
        int newVertexIndex
    ) InsertAndDeleteVertices(
        int targetVerticesLength,
        Vector3 newVertex, 
        List<Vector3> verticesList
    ) {
        int listCount = verticesList.Count;
        int newVertexIndex = listCount;
        bool deltrue = false;
        // 新頂点リストの中に重複する頂点があれば，その頂点のインデックスを返す
        for (int duplicateIndex = 0; duplicateIndex < listCount; duplicateIndex++) {
            if (verticesList[duplicateIndex] == newVertex) {
                newVertexIndex = duplicateIndex;
                deltrue = true;
                break;
            }
        }
        return (deltrue, newVertexIndex + targetVerticesLength);
    }
}

// 切断平面上の頂点と，それらが構成する図形に対する処理系
public static class GeometryUtils {
    // 新頂点リストから，ペア同士の探索を行い，頂点グループを生成する
    public static List<List<int>> GroupingForDetermineGeometry(
        List<int[]> vertexPairList, 
        List<List<int>> joinedVertexGroupList
    ) {
         // コピーのリストを作成
        List<int[]> remainingVertexPairList = new List<int[]>(vertexPairList);
        // 最初のEdgeの開始点と終点を取得
        int startVertex = remainingVertexPairList[0][0];
        int endVertex = remainingVertexPairList[0][1];
        // 最初のEdgeの頂点を追加し、削除
        joinedVertexGroupList[0].Add(startVertex);
        remainingVertexPairList.RemoveAt(0);
        // 頂点が一周するまでループ
        while (startVertex != endVertex) {
            // 残りの頂点リストから、前回の終点から始まるEdgeを探す
            for (int i = 0; i < remainingVertexPairList.Count; i++) {
                if (endVertex == remainingVertexPairList[i][0]) {
                    // 終点を更新、頂点グループに追加し、削除
                    endVertex = remainingVertexPairList[i][1];
                    joinedVertexGroupList[i].Add(endVertex);
                    remainingVertexPairList.RemoveAt(i);
                    break;
                }
            }
        }
        // まだ処理されていない頂点が残っている場合、再帰的にグループ化を続ける
        if (remainingVertexPairList.Count > 0) {
            return GroupingForDetermineGeometry(remainingVertexPairList, joinedVertexGroupList);
        }
        // 全ての頂点ペアが処理された場合、結果を返す
        return joinedVertexGroupList;
    }

    // 図形同士の内外判定を巻き数法 (Winding Number Algorithm) で行い，処理図形ごとにグループ化する
    public static List<List<int>> GroupingForSegmentNonMonotoneGeometry(
        Vector2[] new2DVerticesArray, 
        List<List<int>> joinedVertexGroupList
    ) {
        int groupCount = joinedVertexGroupList.Count;
        Vector2 point = new Vector2 (0, 0);
        // 各図形の内外判定を行うための配列
        bool[][] isInsides = new bool[groupCount][];
        for (int i = 0; i < groupCount; i++) {
            isInsides[i] = new bool[groupCount];
        }
        bool[] visited = new bool[groupCount];
        // 処理図形グループリストに各図形を組み分けするためのリスト
        List<List<int>> nonConvexGeometryList = new List<List<int>>();
        // GroupingForDetermineGeometry で特定された図形の総当たり
        for (int i = 0; i < groupCount; i++) {
            for (int j = 0; j < groupCount; j++) {
                // 自分自身は無視して，他の図形との内外判定を巻き数法で行う
                if (i == j) continue;
                point = new2DVerticesArray[joinedVertexGroupList[j][0]];
                isInsides[i][j] = WindingNumberAlgorithm(new2DVerticesArray, point, joinedVertexGroupList[i]);
            }
        }
        // 図形iが他の図形を内包するしないにかかわらず，非被内包(笑)(処理図形)の場合は，内包図形とともにリストに追加する
        for (int i = 0; i < groupCount; i++) {
            if (visited[i]) continue;
            List<int> group = new List<int>();
            FindOutermostGeometry(isInsides, i, group, visited);
            nonConvexGeometryList.Add(group);
        }
        return nonConvexGeometryList;
    }

    // Winding Number Algorithm の実装
    private static bool WindingNumberAlgorithm(
        Vector2[] new2DVerticesArray, 
        Vector2 point, 
        List<int> toCompareGeometry
    ) {
        // 外郭の辺リストが右回りであることを前提とする
        // 辺の右側が図形の内部になる
        int windingNumber = 0;
        int vertexQuantity = toCompareGeometry.Count;
        for (int i = 0; i < vertexQuantity - 1; i++) {
            Vector2 internalVertex = new2DVerticesArray[toCompareGeometry[i]];
            Vector2 terminalVertex = new2DVerticesArray[toCompareGeometry[i + 1]];

            if (internalVertex.y <= point.y) {
                // 辺の始点が点よりも下・辺の終点が点よりも上・辺の※右側に点がある場合
                if (terminalVertex.y > point.y && IsRight(internalVertex, terminalVertex, point)) {
                    windingNumber--;
                }
            }
            else {
                // 辺の始点が点よりも上・辺の終点が点よりも下・辺の※左側に点がある場合
                if (terminalVertex.y <= point.y && IsLeft(internalVertex, terminalVertex, point)) {
                    windingNumber++;
                }
            }
        }
        // 0 でない場合は内部にある => true
        return windingNumber != 0;
    }

    // GroupingForSegmentNonMonotoneGeometry() の処理図形をグルーピングする補助関数
    private static void FindOutermostGeometry (
        bool[][] isInsides, 
        int index, 
        List<int> group, 
        bool[] visited
    ) {
        // すでにグルーピングした図形は無視する
        if (visited[index]) return;

        visited[index] = true;
        group.Add(index);

        for (int i = 0; i < isInsides.Length; i++) {
            // 図形 i が index に内包されている場合
            if (isInsides[index][i]) {
                FindOutermostGeometry(isInsides, i, group, visited);
            }
            // 図形 index が図形 i に内包されている場合
            else if (isInsides[i][index]) {
                group.Clear();
                FindOutermostGeometry(isInsides, i, group, visited);
                break;
            }
        }
    }

    // 平面上の頂点を2D座標に変換する関数
    public static Vector2[] ConvertCoordinates3DTo2D(
        Plane cutter, 
        List<Vector3> vertices
    ) {
        Vector2[] result = new Vector2[vertices.Count];
        Vector3 planeNormal = cutter.normal;
        Vector3 planePoint = planeNormal * cutter.distance;

        // 法線に垂直なベクトルuを生成
        Vector3 u = Vector3.Cross(planeNormal, Vector3.up).normalized;
        if (u.magnitude < 0.001f) {
            u = Vector3.Cross(planeNormal, Vector3.right).normalized;
        }
        // ベクトルuに垂直なベクトルvを生成
        Vector3 v = Vector3.Cross(planeNormal, u);

        // u, v による座標変換
        for (int i = 0; i < vertices.Count; i++) {
            Vector3 pointOnPlane = vertices[i] - planePoint;
            float x = Vector3.Dot(pointOnPlane, u);
            float y = Vector3.Dot(pointOnPlane, v);
            result[i] = new Vector2(x, y);
        }

        return result;
    }

    // jointedVertexGroupList の各要素の中で，対象の頂点が図形を構成する頂点配列の先頭ならば，図形を構成する頂点の末尾を返す
    public static int HeadJudge(
        List<List<int>> joinedVertexGroupList, 
        int setIndex, 
        int processedTargetVertex
    ) {
        if (joinedVertexGroupList[setIndex][0] == processedTargetVertex) {
            return joinedVertexGroupList[setIndex][joinedVertexGroupList[setIndex].Count - 2];
        }
        // そうでなければ，processedTargetVertex の該当する要素の，次の要素を返す
        else {
            return joinedVertexGroupList[setIndex][joinedVertexGroupList[setIndex].IndexOf(processedTargetVertex) + 1];
        }
        else {
            return processedTargetVertex;
        }
    }
}

// 単調多角形分割と，多角形の三角形分割に関する処理系
public static class ComputationalGeometryAlgorithm {
/* ****************************************************************************** /
* [参考文献]
* コンピュータ・ジオメトリ (計算幾何学: アルゴリズムと応用) ：近代科学社
* M. ドバーグ, M. ファン・クリベルド, M. オーバマーズ, O. シュワルツコップ 共著
* 浅野 哲夫 訳
* ****************************************************************************** */

/* ****************************************************************************** /
* 以下は，凸でない多角形 |P| を三角形分割するためのアルゴリズムである．
* そのために，まずは |P| を単調多角形 (monotone polygon) 配列 |P'|に分割する．
* いきなり |P'| に分割することは困難なので，他の図形に非被内包である，
* 外郭図形 (内包図形の有無に無関係)を，処理図形のグループとして |P_s| にする．
*   ※ 3DCG においての用語との混同を避けるため，以降 monotone geometry とする．
*   ※ |P'| と |P_s| との勲に注意する．
* ここでは，|nonConvexGeometryEdgesJagAry| を疑似的に |P'| として扱う．
* まずは，|P| の頂点を，通常点と変曲点で以下のように分類する．
*   [ 出発点: start, 統合点: merge, 分離点: split, 最終点: end, 通常の点: regular ]
* そして，|P'| の頂点を時計回りに並べたものをv_0, v_1, ..., v_{n-1} とする．
* また，|P'| の辺集合を e_0, e_1, ..., e_{n-1} とする．
* また，|P'| の辺集合と同じ大きさの配列 |Helper| を用意し，
* v_i の頂点種類を，すぐ右の辺にあたる helper(e_{v_i}) に格納する．
* すぐ右の辺がない場合は，自身を終点とする辺の始点が helper(e_{v_i}) となる．
*
* 以下は，そのアルゴリズムの具体的な手順である．
* 1. |P_s| の頂点の分類配列を，y 座標の降順にソートする．(sごとに)
* 2. y 座標の降順に helper(v_i) を参照していき，以下の通りに処理を行う．
*
*    case: 出発点
*    (1). とくに何もしない．
*    case: 最終点
*    (1). もし，helper(e_{v_i}-1) が統合点の場合，
*         - v_i と helper(e_{v_i}-1) を結ぶ両辺を，辺集合 {E_s} に追加する．
*    (2). helper(e_{v_i}-1) を削除する．
*    case: 統合点
*    (1). もし，helper(e_{v_i}-1) が統合点の場合，
*         - v_i と helper(e_{v_i}-1) を結ぶ両辺を，辺集合 {E_s} に追加する．
*    (2). helper(e_{v_i}-1) を削除する．
*    (3). 右隣の辺 e_j を探す．
*    (4). もし，helper(e_j}) が統合点の場合，
*         - v_i と helper(e_j) を結ぶ両辺を，辺集合 {E_s} に追加する．
*    (5). helper(e_j) に v_i を設定する．
*    case: 分離点
*    (1). 右隣の辺 e_j を探す．
*    (2). v_i と helper(e_j) を結ぶ両辺を，辺集合 {E_s} に追加する．
*    (3). helper(e_j) に v_i を設定する．
*    (4). helper(e_{v_i}) を v_i に設定する．
*    case: 通常の点
*    (1). もし，v_i が e_{v_i}-1 の左側にある場合，以下の処理を行う．
*         - もし，helper(e_{v_i}-1) が統合点の場合，
*         -- v_i と helper(e_{v_i}-1) を結ぶ両辺を，辺集合 {E_s} に追加する．
*         -- helper(e_{v_i}-1) を削除する．
*         -- helper(e_{v_i}) に v_i を設定する．
*         - もし，helper(e_{v_i}-1) が統合点でない場合，右隣の辺 e_j を探す．
*         -- もし，helper(e_j) が統合点の場合，
*         --- v_i と helper(e_j) を結ぶ両辺を，辺集合 {E_s} に追加する．
*         -- helper(e_j) に v_i を設定する．
*
* 3. {E_s} ごとに，辺のグルーピングを再度行い，|P'| を生成する．
* 4. |P'| を三角形分割する．
*
* このアルゴリズムで行っていること，図形を分割する際に行わなければいけない処理は，
* 分離点と統合点による，他の頂点からの対角線の横断を防ぐことです．
* 例えば，統合点と分離点が，y 座標で近しい距離にあるとき，
* つまり図形的にわかりやすく判別式を設けるなら，一貫性があればどちらでも構わない，
* すぐ隣の辺の始点から終点の間に存在するほど近しいとき，
* その辺から他の頂点に対して対角線を引くときに，
* 二点による別の対角線が，横断することになります．
* なので，その閾値による存在位置の判別により他の頂点と繋ぐことで，
* 他の頂点からの対角線の横断を防いでいます．
*
* 最後に，このアルゴリズムは参考文献をもとに，自己流にアレンジしたものなので，
* 参考文献とは重点を置いている部分が異なります．
* 図形探索アルゴリズムに最適はあるかもしれないけど正解はないよね？
* 参考文献にだっていやちょっと待て，と，そういう部分もあるしね．
* ****************************************************************************** */

    // 処理図形グループの頂点配列を，頂点の種類に応じて探索する
    public static void MakeMonotone(
        Vector2[] new2DVerticesArray, 
        List<List<int>> joinedVertexGroupList, 
        List<List<int>> nonConvexGeometryList
    ) {
        // 処理図形グループの数
        int processingCount = nonConvexGeometryList.Count;
        // 新頂点の種類を格納する配列
        string[] vertexType = new string[new2DVerticesArray.Length];
        // 処理図形グループ n のノード配列
        int[][] part_nonConvexGeometryNodesJagAry;
        // 処理図形グループ n の辺リスト
        List<int[]> part_nonConvexGeometryEdgesList;
        // 各辺のヘルパー配列
        int[] helper = new int[new2DVerticesArray.Length];
        for (int i = 0; i < helper.Length; i++) {
            helper[i] = i;
        }

        // 新頂点を種類ごとに分類する
        vertexType = ClusteringVertexType(
            new2DVerticesArray, 
            joinedVertexGroupList, 
        );

        // 処理図形グループごとに，単調多角形分割を行う
        for (int i = 0; i < processingCount; i++) {
            // 頂点配列と辺配列を生成する
            (
                part_nonConvexGeometryVerticesJagAry, 
                part_nonConvexGeometryNodesJagAry
            ) = EdgeForMakeMonotone(
                processingCount, 
                nonConvexGeometryList, 
                joinedVertexGroupList
            );
            // 頂点配列の y 座標ソート順 (降順) を取得する
            alignment = SortVerticesByCoordinateY(
                new2DVerticesArray, 
                nonConvexGeometryList, 
                joinedVertexGroupList
            );
            // 単調多角形分割を行う
            PreMakeMonotone(
                new2DVerticesArray, 
                vertexType, 
                helper,
                part_nonConvexGeometryNodesJagAry,
                part_nonConvexGeometryEdgesList
            );
        }
    }

    // 処理図形グループのうちの一つのグループの頂点配列を，頂点の種類に応じて探索する
    private static void PreMakeMonotone(
        Vector2[] new2DVerticesArray, 
        string[] vertexType, 
        int[] helper,
        int[][] part_nonConvexGeometryNodesJagAry, 
        List<int[]> part_nonConvexGeometryEdgesList
    ) {
        // alignment を使って，y 座標の降順に頂点配列を探索する．
        for (int i = 0; i < alignment.Length; i++) {
            // 対角線を構成する頂点のインデックス
            int diagonalVertex1 = new int();
            int diagonalVertex2 = new int();
            // 探索対象の頂点の種類を取得する
            string targetVertexType = new string(vertexType[part_nonConvexGeometryNodesJagAry[i][1]]);
            // 出発点の場合は，特に何もしない
            // 最終点の場合
            if (targetVertexType == "end") {
                HandleEndVertex();
            }
            // 統合点の場合
            else if (targetVertexType == "merge") {
                HandleMergeVertex();
            }
            // 分離点の場合
            else if (targetVertexType == "split") {
                HandleSplitVertex();
            }
            // 通常の点の場合
            else if (targetVertexType == "regular") {
                HandleRegularVertex();
            }
        }
    }

    // 処理図形ごとの頂点ペアのリストを生成する
    private static (
        int [][] part_nonConvexGeometryNodesJagAry,
        List<int[]> part_nonConvexGeometryEdgesList
    ) EdgeForMakeMonotone(
        int i, 
        List<List<int>> nonConvexGeometryList, 
        List<List<int>> joinedVertexGroupList
    ) {
        /*
        *   jointedVertexGroupList の返却型をはじめから
        *   List<List<int[]>> にする方法とどちらが良いかは感覚でこちらにした．
        *
        *   List<List<int>> A { // nonConvexGeometryList
        *       List<int> [0] {0, 1, 3},
        *       List<int> [1] {4, 2}
        *   }
        *   List<List<int>> B { // joinedVertexGroupList
        *       List<int> [0] {24, 25, 26, 27, 24},             // 処理図形グループ 1
        *       List<int> [1] {28, 29, 30, 31, 32, 28},         // 処理図形グループ 2
        *       List<int> [2] {33, 34, 35, 36, 37, 33},         // 処理図形グループ 1
        *       List<int> [3] {38, 39, 40, 38}                  // 処理図形グループ 1
        *       List<int> [4] {41, 42, 43, 44, 45, 46, 47, 41}  // 処理図形グループ 2
        *   }
        *   // この二つから，各処理図形ごとの辺リストを生成する
        *   int[][][] C = new int[A.Count][][] {
        *       int[0] = new int[ for (i = A[0].Count)(j = A[i].Count) {B[A[i][j]].Count - 1} sum+= ][2] {
        *           {24, 25}, {25, 26}, {26, 27}, {27, 24}, 
        *           {28, 29}, {29, 30}, {30, 31}, {31, 32}, {32, 28}, 
        *           {38, 39}, {39, 40}, {40, 38}
        *       },
        *       int[1] = new int[ for (i = A[1].Count)(j = A[i].Count) {B[A[i][j]].Count - 1} sum+= ][2] {
        *           {41, 42}, {42, 43}, {43, 44}, {44, 45}, {45, 46}, {46, 47}, {47, 41}, 
        *           {33, 34}, {34, 35}, {35, 36}, {36, 37}, {37, 33}
        *       }
        *   }
        */

        int total = 0;
        int index = 0;
        //グループに必要な総頂点数・エッジ数を計算する
        for (int j = 0; j < nonConvexGeometryList[i].Count; j++) {
            total += joinedVertexGroupList[nonConvexGeometryList[i][j]].Count - 1;
        }
        // グループのノード配列を初期化する
        int[][] part_nonConvexGeometryNodesJagAry = new int[total][];
        // グループの辺リストを初期化する
        List<int[]> part_nonConvexGeometryEdgesList = new List<int[2]>();

        // 頂点リスト・辺リストに値と参照を代入する
        for (int j = 0; j < nonConvexGeometryList[i].Count; j++) {
            List<int> vertexIndices = joinedVertexGroupList[nonConvexGeometryList[i][j]];
            for (int k = 0; k < vertexIndices.Count - 1; k++) {
                part_nonConvexGeometryNodesJagAry[index] = new int[3];
                part_nonConvexGeometryNodesJagAry[index][0] = k == 0 ? vertexIndices[vertexIndices.Count - 2] : vertexIndices[k - 1];
                part_nonConvexGeometryNodesJagAry[index][1] = vertexIndices[k];
                part_nonConvexGeometryNodesJagAry[index][2] = vertexIndices[k + 1];
                part_nonConvexGeometryEdgesList.Add(new int[2] { vertexIndices[k], vertexIndices[k + 1] });
                index++;
            }
        }
        return (part_nonConvexGeometryNodesJagAry, part_nonConvexGeometryEdgesList);
    }

    // 頂点の種類を判別して，各頂点にラベルを付与する
    private static string[] ClusteringVertexType(
        Vector2[] new2DVerticesArray, 
        List<List<int>> joinedVertexGroupList
    ) {
        // 頂点の種類を格納する配列を新頂点の数と同じ大きさで用意する
        string[] vertexType = new string[new2DVerticesArray.Length];
        // GroupingForDetermineGeometry() で特定された図形ごとに頂点ラベル処理を行う
        for (int i = 0; i < joinedVertexGroupList.Count; i++) {
            for (int j = 0; j < joinedVertexGroupList[i].Count - 1; j++) {
                Vector2 internalVertex = new2DVerticesArray[joinedVertexGroupList[i][j]];
                Vector2 terminalVertex = new2DVerticesArray[joinedVertexGroupList[i][j + 1]];
                Vector2 point = j == 0 ? new2DVerticesArray[joinedVertexGroupList[i].Count - 2] : new2DVerticesArray[joinedVertexGroupList[i][j - 1]];
                // y座標が前後の頂点と比較して対象の点が大きいとき
                if (internalVertex.y >= point.y && internalVertex.y > terminalVertex.y) {
                    // 部分最大の場合: 出発点
                    if (MathUtils.IsRight(internalVertex, terminalVertex, point)) {
                        vertexType[joinedVertexGroupList[i][j]] = "start";
                    }
                    // 部分極大の場合: 分離点
                    else {
                        vertexType[joinedVertexGroupList[i][j]] = "split";
                    }
                }
                // y座標が前後の頂点と比較して対象の点が小さいとき
                else if (internalVertex.y <= point.y && internalVertex.y < terminalVertex.y) {
                    // 部分最小の場合: 最終点
                    if (MathUtils.IsRight(internalVertex, terminalVertex, point)) {
                        vertexType[joinedVertexGroupList[i][j]] = "end";
                    }
                    // 部分極小の場合: 統合点
                    else {
                        vertexType[joinedVertexGroupList[i][j]] = "merge";
                    }
                }
                // それ以外の場合: 通常の点
                else {
                    vertexType[joinedVertexGroupList[i][j]] = "regular";
                }
            }
        }
        return vertexType;
    }

    // ヘルパ頂点配列と新頂点配列を関連付けて，処理図形グループ頂点リストを y 座標の降順にソートしたときの，ソート前のインデックス情報をもつ配列を生成する (※ソートは行わない．あくまでもソートした仮定での順番を格納するだけ)
    private static void SortVerticesByCoordinateY(
        Vector2[] new2DVerticesArray, 
        int[][] part_nonConvexGeometryNodesJagAry
    ) {
        // 配列を対応する頂点の y座標 > x座標 の優先度で降順にソート
        Array.Sort(part_nonConvexGeometryNodesJagAry, (a, b) => {
            // y 座標を比較（降順）
            int compareY = new2DVerticesArray[part_nonConvexGeometryNodesJagAry[b][1]].y.CompareTo(new2DVerticesArray[part_nonConvexGeometryNodesJagAry[a][1]].y);
            if (compareY != 0) {
                return compareY;
            }
            // y 座標が等しければ x 座標を比較（降順）
            return new2DVerticesArray[part_nonConvexGeometryNodesJagAry[b][1]].x.CompareTo(new2DVerticesArray[part_nonConvexGeometryNodesJagAry[a][1]].x);
        });
    }

    // 対象の頂点が最終点である場合の処理
    private static void HandleEndVertex(
        int i, 
        Vector2[] new2DVerticesArray, 
        string[] vertexType, 
        int[] helper,
        int[][] part_nonConvexGeometryNodesJagAry, 
        List<int[]> part_nonConvexGeometryEdgesList
    ) {
        // もし，helper(e_{v_i}-1) が統合点の場合，
        if (vertexType[part_nonConvexGeometryNodesJagAry[alignment[i]][0]] == "merge") {
            // v_i と helper(e_{v_i}-1) を結ぶ両辺を，辺集合 {E_s} に追加する
            part_nonConvexGeometryEdgesList.Add(new int[2] { part_nonConvexGeometryNodesJagAry[i][0], part_nonConvexGeometryNodesJagAry[i][1] });
            part_nonConvexGeometryEdgesList.Add(new int[2] { part_nonConvexGeometryNodesJagAry[i][1], part_nonConvexGeometryNodesJagAry[i][0] });
        }
    }

    // 対象の頂点が統合点である場合の処理
    private static void HandleMergeVertex(
        int i, 
        Vector2[] new2DVerticesArray, 
        string[] vertexType, 
        int[] helper,
        int[][] part_nonConvexGeometryNodesJagAry, 
        List<int[]> part_nonConvexGeometryEdgesList
    ) {
        // もし，helper(e_{v_i}-1) が統合点の場合，
        if (vertexType[part_nonConvexGeometryNodesJagAry[alignment[i]][0]] == "merge") {
            // v_i と helper(e_{v_i}-1) を結ぶ両辺を，辺集合 {E_s} に追加する
            part_nonConvexGeometryEdgesList.Add(new int[2] { part_nonConvexGeometryNodesJagAry[i][0], part_nonConvexGeometryNodesJagAry[i][1] });
            part_nonConvexGeometryEdgesList.Add(new int[2] { part_nonConvexGeometryNodesJagAry[i][1], part_nonConvexGeometryNodesJagAry[i][0] });
        }
        // すぐ右隣の辺を探す
        for (int j = 0; j < part_nonConvexGeometryNodesJagAry.Length; j++) {
            if (new2DVerticesArray[part_nonConvexGeometryNodesJagAry[j][1]].y > new2DVerticesArray[part_nonConvexGeometryNodesJagAry[i][1]].y && new2DVerticesArray[part_nonConvexGeometryNodesJagAry[j][2]].y <= new2DVerticesArray[part_nonConvexGeometryNodesJagAry[i][1]].y) {
                // もし，helper(e_j) が統合点の場合，
                if (vertexType[part_nonConvexGeometryNodesJagAry[j][2]] == "merge") {
                    // v_i と helper(e_j) を結ぶ両辺を，辺集合 {E_s} に追加する
                    part_nonConvexGeometryEdgesList.Add(new int[2] { part_nonConvexGeometryNodesJagAry[i][1], part_nonConvexGeometryNodesJagAry[j][2] });
                    part_nonConvexGeometryEdgesList.Add(new int[2] { part_nonConvexGeometryNodesJagAry[j][2], part_nonConvexGeometryNodesJagAry[i][1] });
                }
                // helper(e_j) に v_i を設定する
                part_nonConvexGeometryNodesJagAry[j][2] = part_nonConvexGeometryNodesJagAry[i][1];
                break;
            }
        }
    }

    // 対象の頂点が分離点である場合の処理
    private static void HandleSplitVertex(
        int i, 
        Vector2[] new2DVerticesArray, 
        string[] vertexType, 
        int[] helper,
        int[][] part_nonConvexGeometryNodesJagAry, 
        List<int[]> part_nonConvexGeometryEdgesList
    ) {
        // 右隣の辺を探す
        for (int j = 0; j < part_nonConvexGeometryNodesJagAry.Length; j++) {
            if (new2DVerticesArray[part_nonConvexGeometryNodesJagAry[j][1]].y > new2DVerticesArray[part_nonConvexGeometryNodesJagAry[i][1]].y && new2DVerticesArray[part_nonConvexGeometryNodesJagAry[j][2]].y <= new2DVerticesArray[part_nonConvexGeometryNodesJagAry[i][1]].y) {
                // v_i と helper(e_j) を結ぶ両辺を，辺集合 {E_s} に追加する
                part_nonConvexGeometryEdgesList.Add(new int[2] { part_nonConvexGeometryNodesJagAry[i][1], part_nonConvexGeometryNodesJagAry[j][2] });
                part_nonConvexGeometryEdgesList.Add(new int[2] { part_nonConvexGeometryNodesJagAry[j][2], part_nonConvexGeometryNodesJagAry[i][1] });
                // helper(e_j) に v_i を設定する
                part_nonConvexGeometryNodesJagAry[j][2] = part_nonConvexGeometryNodesJagAry[i][1];
                break;
            }
        }


    }

    // 対象の頂点が通常点である場合の処理
    private static void HandleRegularVertex(
        int i, 
        Vector2[] new2DVerticesArray, 
        string[] vertexType, 
        int[] helper,
        int[][] part_nonConvexGeometryNodesJagAry, 
        List<int[]> part_nonConvexGeometryEdgesList
    ) {

    }
}

// 計算に関する処理系
public static class MathUtils {
    // 2つのベクトルの外積を計算する
    public static float CrossProduct(
        Vector2 internalVertex, 
        Vector2 terminalVertex, 
        Vector2 point
    ) {
        Vector2 v1 = terminalVertex - internalVertex;
        Vector2 v2 = point - internalVertex;
        return v1.x * v2.y - v1.y * v2.x;
    }

    // 頂点が右回りであることが前提
    public static bool IsRight(
        Vector2 internalVertex, 
        Vector2 terminalVertex, 
        Vector2 point
    ) {
        return CrossProduct(internalVertex, terminalVertex, point) < 0;
    }

    // 頂点が右回りであることが前提
    public static bool IsLeft(
        Vector2 internalVertex, 
        Vector2 terminalVertex, 
        Vector2 point
    ) {
        return CrossProduct(internalVertex, terminalVertex, point) > 0;
    }
}

public class RefInt {
    public int Value { get; set; }
    public RefInt(int value) {
        Value = value;
    }
}

// Debug用の処理系
public static class MyDebug {
    public void CreateObject(
        Vector3[] vertices, 
        Vector2[] uvs, 
        int[] triangles
    ) {
        GameObject newObject = Instantiate(newGameObjectPrefab);
        newObject.AddComponent<MeshFilter>();
        newObject.AddComponent<MeshRenderer>();
        Mesh mesh = newObject.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.uv = uvs;
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }

    public void CreateRigidObject(
        Vector3[] vertices, 
        Vector2[] uvs, 
        int[] triangles
    ) {
        GameObject newObject = Instantiate(newGameObjectPrefab);
        newObject.AddComponent<MeshFilter>();
        newObject.AddComponent<MeshRenderer>();
        Rigidbody rigid = newObject.AddComponent<Rigidbody>();
        Mesh mesh = newObject.GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.uv = uvs;
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }
}