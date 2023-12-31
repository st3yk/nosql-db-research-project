# Bucket to store website

resource "google_compute_address" "db_vm1_static_ip" {
  name = "db-vm1-static-ip"
  address_type = "EXTERNAL"
}

resource "google_compute_instance" "db_vm1" {
  name         = "db-vm1"
  machine_type = "e2-standard-4"
  zone = var.gcp_zone
  tags = ["db"]
  metadata = {
    ssh-keys = "tymon_szczepanowski:${file("~/.ssh/id_ed25519.pub")}"
  }
  boot_disk {
    initialize_params {
      image = "almalinux-cloud/almalinux-8"
    }
  }
  network_interface {
    network = "default"
    access_config {
      nat_ip = google_compute_address.db_vm1_static_ip.address
    }
  }
}

resource "google_compute_firewall" "default" {
 name    = "web-firewall"
 network = "default"

 allow {
   protocol = "tcp"
   ports    = ["27017"]
 }

 source_ranges = ["0.0.0.0/0"]
 target_tags = ["db"]
}