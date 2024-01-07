# Provisioning VMs for benchmark + db cluster
resource "google_compute_address" "benchmark_static_ip" {
  name = "benchmark-static-ip"
  address_type = "EXTERNAL"
}

resource "google_compute_instance" "benchmark" {
  name         = "benchmark"
  machine_type = "e2-medium"
  zone = var.gcp_zone
  metadata = {
    ssh-keys = "tymon_szczepanowski:${file("~/.ssh/id_ed25519.pub")}\njan_marciniak125:${file("~/.ssh/janek.pub")}\npoprostubartek_official:${file("~/.ssh/bartek.pub")}"
  }
  boot_disk {
    initialize_params {
      image = "almalinux-cloud/almalinux-8"
    }
  }
  network_interface {
    network = "default"
    access_config {
      nat_ip = google_compute_address.benchmark_static_ip.address
    }
  }
}

resource "google_compute_address" "db_vm_static_ip" {
  count = 3
  name = "db-vm-${format("%d", count.index + 1)}-static-ip"
  address_type = "EXTERNAL"
}

resource "google_compute_instance" "db_vm" {
  count = 3
  name = "db-vm-${format("%d", count.index + 1)}"
  machine_type = "e2-standard-2"
  zone = var.gcp_zone
  tags = ["db"]
  metadata = {
    ssh-keys = "tymon_szczepanowski:${file("~/.ssh/id_ed25519.pub")}\njan_marciniak125:${file("~/.ssh/janek.pub")}\npoprostubartek_official:${file("~/.ssh/bartek.pub")}"
  }
  boot_disk {
    initialize_params {
      image = "almalinux-cloud/almalinux-8"
    }
  }
  network_interface {
    network = "default"
    access_config {
      nat_ip = google_compute_address.db_vm_static_ip["${count.index}"].address
    }
  }
}