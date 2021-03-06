﻿using Microsoft.Extensions.Options;
using SHPA.Blockchain.Actions.Models;
using SHPA.Blockchain.Blocks;
using SHPA.Blockchain.Client;
using SHPA.Blockchain.Configuration;
using SHPA.Blockchain.Server.ActionResult;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SHPA.Blockchain.Nodes
{
    public class NodeManager : INodeManager
    {
        private readonly NodeConfiguration _option;
        private readonly Dictionary<string, Node> _nodes;
        private readonly Node _node;

        public NodeManager(IOptions<NodeConfiguration> option)
        {
            _option = option.Value;
            _nodes = new Dictionary<string, Node>();
            _node = new Node(new Uri(_option.GetFullAddress()), _option.Name);
        }

        public void Ping()
        {
            var watch = new Stopwatch();
            foreach (var node in _nodes)
            {
                watch.Start();
                var ping = RestClient.Make(node.Value.Address).Get().Execute<JsonResultModel<PingResultModel>>("ping");
                var elapsedTime = watch.ElapsedMilliseconds;
                watch.Reset();
                if (ping != null && ping.Success)
                {
                    Console.WriteLine($"Reply from {node.Key} is success: time={elapsedTime}ms, return_node_name:{ping.Result.NodeName}");
                }
                else
                {
                    Console.WriteLine($"Reply from {node.Key} is failed: time={elapsedTime}ms");
                }
            }
        }



        public (bool Result, string Message) RegisterNode(Node node)
        {
            if (_nodes.Count >= _option.MaxNodeCapacity)
                return (false, "maximum capacity is exceeded");
            if (_nodes.ContainsKey(node.Name))
                return (false, $"node {node.Name} was registered previously");

            var ping = RestClient.Make(node.Address).Get().Execute<JsonResultModel<PingResultModel>>("ping");
            if (ping == null || !ping.Success)
                return (false, $"node {node.Name} is not reachable");
            if (!ping.Result.NodeName.Equals(node.Name))
                return (false, $"registered node name {node.Name} is not equal to ping result node name {ping.Result.NodeName}");

            _nodes.Add(node.Name, node);
            return (true, string.Empty);
        }

        public Node[] GetRegisterNodes()
        {
            return _nodes.Values.ToArray();
        }

        public Node Node() => _node;

        public (bool Result, string[] errors) BroadcastNewBlock(Block<Transaction> input)
        {
            var errors = new List<string>();
            foreach (var node in _nodes)
            {
                var result = RestClient.Make(node.Value.Address).Post().AddBody(input).Execute<JsonResultModel<bool>>("addblock");
                if (!result.Success)
                    errors.AddRange(result.Errors);
            }

            if (errors.Any())
                return (false, errors.ToArray());
            return (true, null);
        }
    }
}