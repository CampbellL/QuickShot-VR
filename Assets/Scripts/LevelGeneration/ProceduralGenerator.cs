using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace LevelGeneration
{
    [Serializable]
    public struct ProceduralGeneratorSetup
    {
        public int chunkSize;
        public int pieceSize;
        public GameObject levelParent;
        public List<GameObject> pieces;
        public NavMeshSurface navMesh;
    }

    public class ProceduralGenerator
    {
        private readonly List<GameObject> _pieces;
        private readonly NavMeshSurface _navMesh;
        private readonly int _chunkSize;
        private int _refreshCounter;
        private readonly int _pieceSize;
        private int _lastChunkZ = 20;
        private readonly GameObject _levelParent;
        private readonly Queue<GameObject> _chunk = new Queue<GameObject>();

        public ProceduralGenerator(IObservable<bool> levelRefreshSubject, ProceduralGeneratorSetup options)
        {
            this._navMesh = options.navMesh;
            this._pieces = options.pieces;
            this._chunkSize = options.chunkSize;
            this._pieceSize = options.pieceSize;
            this._levelParent = options.levelParent ? options.levelParent : new GameObject("Level Parent");
            for (var i = 0; i < 2; i++)
            {
                MainThreadDispatcher.StartUpdateMicroCoroutine(GenerateChunk());
            }

            levelRefreshSubject.Subscribe(_ =>
            {
                this._refreshCounter++;
                if (this._refreshCounter != _chunkSize) return;
                this.ClearPreviousChunk();
                MainThreadDispatcher.StartUpdateMicroCoroutine(this.GenerateChunk());
                this._refreshCounter = 0;
            });
        }

        private void ClearPreviousChunk()
        {
            for (var i = 0; i <= _chunk.Count - _chunkSize / 2 - 1; i++)
            {
                //Object.Destroy(this._chunk.Peek());
                ObjectPooler.Instance.StartCoroutine(ObjectPooler.Instance.CustomDestroy(this._chunk.Peek()));
                this._chunk.Dequeue();
            }
        }

        private IEnumerator GenerateChunk()
        {
            Vector3 position = new Vector3(0, 0, 0);
            for (var i = 0; i < _chunkSize; i++)
            {
                position.z = _lastChunkZ + _pieceSize * i;
                //this._chunk.Enqueue(Object.Instantiate(_pieces[Random.Range(0, _pieces.Count)], position, Quaternion.identity, _levelParent.transform));
                this._chunk.Enqueue(ObjectPooler.Instance.SpawnRandomChunk(position, Quaternion.identity, _levelParent.transform));
            }

            _lastChunkZ += _chunkSize * _pieceSize;
            //this._navMesh.BuildNavMesh();
            yield return null;
        }
    }
}