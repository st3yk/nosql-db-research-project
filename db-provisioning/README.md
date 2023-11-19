## Database provisioning
For tests use Vagrant environment
```shell
# Create VMs
user@host:~/research-project/db-provisioning$ vagrant up
# Destroy them
user@host:~/research-project/db-provisioning$ vagrant destroy -f
# Connect via ssh
user@host:~/research-project/db-provisioning$ vagrant ssh db-vm-1
```
### Install MongoDB (+replicaset)
```shell
user@host:~/research-project/db-provisioning$ ansible-playbook -i inventory.yaml playbooks/install-mongo.yaml
user@host:~/research-project/db-provisioning$ ansible-playbook -i inventory.yaml playbooks/setup-mongo-replicaset.yaml
```
### Install Cassandra
```shell
user@host:~/research-project/db-provisioning$ ansible-playbook -i inventory.yaml playbooks/install-cassandra.yaml
```