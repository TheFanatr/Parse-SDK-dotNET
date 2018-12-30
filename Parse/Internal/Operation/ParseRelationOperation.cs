using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Parse.Core.Internal
{
    public class ParseRelationOperation : IParseFieldOperation
    {
        IList<string> AdditionQueue { get; }
        IList<string> RemovalQueue { get; }

        ParseRelationOperation(IEnumerable<string> itemsToAdd, IEnumerable<string> itemsToRemove, string targetClassName)
        {
            TargetClassName = targetClassName;
            AdditionQueue = new ReadOnlyCollection<string>(itemsToAdd.ToList());
            RemovalQueue = new ReadOnlyCollection<string>(itemsToRemove.ToList());
        }

        public ParseRelationOperation(IEnumerable<ParseObject> objectsToAdd, IEnumerable<ParseObject> objectsToRemove)
        {
            TargetClassName = (objectsToAdd = objectsToAdd ?? new ParseObject[0]).Concat(objectsToRemove = objectsToRemove ?? new ParseObject[0]).Select(o => o.ClassName).FirstOrDefault();
            AdditionQueue = new ReadOnlyCollection<string>(IdsFromObjects(objectsToAdd).ToList());
            RemovalQueue = new ReadOnlyCollection<string>(IdsFromObjects(objectsToRemove).ToList());
        }

        public object Encode()
        {
            List<object> additionQueue = AdditionQueue.Select(id => PointerOrLocalIdEncoder.Instance.Encode(ParseObject.CreateWithoutData(TargetClassName, id))).ToList();
            List<object> removalQueue = RemovalQueue.Select(id => PointerOrLocalIdEncoder.Instance.Encode(ParseObject.CreateWithoutData(TargetClassName, id))).ToList();

            Dictionary<string, object> additionOperation = additionQueue.Count == 0 ? null : new Dictionary<string, object>
            {
                ["__op"] = "AddRelation",
                ["objects"] = additionQueue
            };
            Dictionary<string, object> removalOperation = removalQueue.Count == 0 ? null : new Dictionary<string, object>
            {
                ["__op"] = "RemoveRelation",
                ["objects"] = removalQueue
            };

            return additionOperation != null && removalOperation != null ? new Dictionary<string, object> { ["__op"] = "Batch", ["ops"] = new[] { additionOperation, removalOperation } } : additionOperation ?? removalOperation;
        }

        public IParseFieldOperation MergeWithPrevious(IParseFieldOperation previous)
        {
            switch (previous)
            {
                case null:
                    return this;
                case ParseDeleteOperation _:
                    throw new InvalidOperationException("You can't modify a relation after deleting it.");
                case ParseRelationOperation other:
                    if (other.TargetClassName != TargetClassName)
                        throw new InvalidOperationException(String.Format("Related object must be of class {0}, but {1} was passed in.", other.TargetClassName, TargetClassName));

                    return new ParseRelationOperation(AdditionQueue.Union(other.AdditionQueue.Except(RemovalQueue)).ToList(), RemovalQueue.Union(other.RemovalQueue.Except(AdditionQueue)).ToList(), TargetClassName);
                default:
                    throw new InvalidOperationException("Operation is invalid after previous operation.");
            }
        }

        public object Apply(object oldValue, string key)
        {
            if (AdditionQueue.Count == 0 && RemovalQueue.Count == 0)
                return null;

            switch (oldValue)
            {
                case null:
                    return ParseRelationBase.CreateRelation(null, key, TargetClassName);
                case ParseRelationBase oldRelation:
                {
                    if (oldRelation.TargetClassName is string oldClassName && oldClassName != TargetClassName)
                        throw new InvalidOperationException($"Related object must be a {oldClassName}, but a {TargetClassName} was passed in.");

                    oldRelation.TargetClassName = TargetClassName;
                    return oldRelation;
                }
                default:
                    throw new InvalidOperationException("Operation is invalid after previous operation.");
            }
        }

        public string TargetClassName { get; }

        IEnumerable<string> IdsFromObjects(IEnumerable<ParseObject> objects)
        {
            foreach (ParseObject obj in objects)
            {
                if (obj.ObjectId == null)
                    throw new ArgumentException("You can't add an unsaved ParseObject to a relation.");
                if (obj.ClassName != TargetClassName)
                    throw new ArgumentException(String.Format("Tried to create a ParseRelation with 2 different types: {0} and {1}", TargetClassName, obj.ClassName));
            }
            return objects.Select(o => o.ObjectId).Distinct();
        }
    }
}
